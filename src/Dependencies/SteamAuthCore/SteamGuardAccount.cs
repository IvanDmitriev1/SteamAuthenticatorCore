using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using SteamAuthCore.Exceptions;
using SteamAuthCore.Models;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore
{
    public class SteamGuardAccount : IDisposable
    {
        #region Properties

        [JsonPropertyName("shared_secret")]
        public string? SharedSecret { get; set; }

        [JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; } = null!;

        [JsonPropertyName("revocation_code")]
        public string RevocationCode { get; set; } = null!;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = null!;

        [JsonPropertyName("server_time")]
        public long ServerTime { get; set; }

        [JsonPropertyName("account_name")]
        public string AccountName { get; set; } = null!;

        [JsonPropertyName("token_gid")]
        public string TokenGid { get; set; } = null!;

        [JsonPropertyName("identity_secret")]
        public string IdentitySecret { get; set; } = null!;

        [JsonPropertyName("secret_1")]
        public string Secret1 { get; set; } = null!;

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = null!;

        /// <summary>
        /// Set to true if the authenticator has actually been applied to the account.
        /// </summary>
        [JsonPropertyName("fully_enrolled")]
        public bool FullyEnrolled { get; set; }

        public SessionData Session { get; set; } = null!;

        #endregion

        private static readonly byte[] SteamGuardCodeTranslations = { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };
        private static readonly HtmlParser Parser = new();
        private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create();

        private HMACSHA1? _hmacGenerator;

        public void Dispose()
        {
            _hmacGenerator?.Dispose();
        }

        public override bool Equals(object obj)
        {
            if (obj is not SteamGuardAccount account) return false;

            return account.Secret1 == this.Secret1 && account.IdentitySecret == this.IdentitySecret;
        }

        

        public async Task<bool> DeactivateAuthenticator(int scheme = 2)
        {
            var postData = new NameValueCollection
            {
                {"steamid", Session.SteamId.ToString()},
                {"steamguard_scheme", scheme.ToString()},
                {"revocation_code", RevocationCode},
                {"access_token", Session.OAuthToken}
            };

            return await SteamApi.MobileLoginRequest<RemoveAuthenticatorResponse>(
                ApiEndpoints.SteamApiBase + "/ITwoFactorService/RemoveAuthenticator/v0001",
                SteamApi.RequestMethod.Post, postData) is not { Response: { Success: true } };
        }

        public string? GenerateSteamGuardCode(Int64 time)
        {
            if (_hmacGenerator is null)
            {
                if (string.IsNullOrEmpty(SharedSecret))
                    return null;

                var sharedSecretUnescaped = Regex.Unescape(SharedSecret!);
                var sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
                _hmacGenerator = new HMACSHA1(sharedSecretArray);
            }

            var timeArray = ArrayPool.Rent(8);

            time /= 30L;

            for (int i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            var hashedData= _hmacGenerator.ComputeHash(timeArray, 0, 8);

            Span<byte> codeArray = stackalloc byte[5];
            try
            {
                byte b = (byte)(hashedData[19] & 0xF);
                int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

                for (int i = 0; i< 5; ++i)
                {
                    codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                    codePoint /= SteamGuardCodeTranslations.Length;
                }
            }
            catch (Exception)
            {
                ArrayPool.Return(timeArray, true);
                return null; //Change later, catch-alls are bad!
            }

            ArrayPool.Return(timeArray, true);
            return Encoding.UTF8.GetString(codeArray);
        }

        public async Task<IEnumerable<ConfirmationModel>> FetchConfirmationsAsync()
        {
            string url = GenerateConfirmationUrl();
            return FetchConfirmationInternal(await SteamApi.RequestAsync(url, SteamApi.RequestMethod.Get, "", Session.GetCookies()));
        }

        public bool SendConfirmationAjax(ConfirmationModel conf, ConfirmationOptions op)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/ajaxop";
            string queryString = "?op=" + op.ToString().ToLower() + "&";
            queryString += GenerateConfirmationQueryParams(op.ToString().ToLower());
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;

            return SteamApi.Request<SendConfirmationResponse>(url, SteamApi.RequestMethod.Get, "", Session.GetCookies(), null) is
            {
                Success: true
            };
        }

        public bool SendConfirmationAjax(IEnumerable<ConfirmationModel> confs, ConfirmationOptions op)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/multiajaxop";

            string query = "op=" + op.ToString().ToLower() + "&" + GenerateConfirmationQueryParams(op.ToString().ToLower());
            foreach (var conf in confs)
            {
                query += "&cid[]=" + conf.Id + "&ck[]=" + conf.Key;
            }

            return SteamApi.Request<SendConfirmationResponse>(url, SteamApi.RequestMethod.Post, query, Session.GetCookies()) is
            {
                Success: true
            };
        }

        public ConfirmationDetailsResponse? GetConfirmationDetails(ConfirmationModel conf)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/details/" + conf.Id + "?";
            string queryString = GenerateConfirmationQueryParams("details");
            url += queryString;

            return SteamApi.Request<ConfirmationDetailsResponse>(url, SteamApi.RequestMethod.Get, "", Session.GetCookies());
        }

        [Obsolete]
        public async Task<bool> RefreshSessionAsync()
        {
            string url = ApiEndpoints.MobileauthGetwgtoken;
            NameValueCollection postData = new NameValueCollection {{"access_token", Session.OAuthToken}};

            if (await SteamApi.RequestAsync<RefreshSessionDataResponse>(url, SteamApi.RequestMethod.Post, postData) is not { Response: not null } refreshResponse)
                return false;

            string token = Session.SteamId + "%7C%7C" + refreshResponse.Response.Token;
            string tokenSecure = Session.SteamId + "%7C%7C" + refreshResponse.Response.TokenSecure;

            Session.SteamLogin = token;
            Session.SteamLoginSecure = tokenSecure;
            return true;
        }

        public string GenerateConfirmationUrl(string tag = "conf")
        {
            string endpoint = ApiEndpoints.CommunityBase + "/mobileconf/conf?";
            string queryString = GenerateConfirmationQueryParams(tag);
            return endpoint + queryString;
        }

        public string GenerateConfirmationQueryParams(string tag)
        {
            if (string.IsNullOrEmpty(DeviceId))
                throw new ArgumentException("Device ID is not present");

            var queryParams = GenerateConfirmationQueryParamsAsNvc(tag);

            return "p=" + queryParams["p"] + "&a=" + queryParams["a"] + "&k=" + queryParams["k"] + "&t=" + queryParams["t"] + "&m=android&tag=" + queryParams["tag"];
        }

        public NameValueCollection GenerateConfirmationQueryParamsAsNvc(string tag)
        {
            if (string.IsNullOrEmpty(DeviceId))
                throw new ArgumentException("Device ID is not present");

            //TODO var time = TimeAligner.GetSteamTime();

            var time = 0;

            var ret = new NameValueCollection
            {
                {"p", DeviceId},
                {"a", Session.SteamId.ToString()},
                {"k", GenerateConfirmationHashForTime(time, tag)},
                {"t", time.ToString()},
                {"m", "android"},
                {"tag", tag}
            };

            return ret;
        }


        #region PrivateMethods

        private static IEnumerable<ConfirmationModel> FetchConfirmationInternal(string? response)
        {
            if (response is null)
                throw new WgTokenInvalidException();

            if (response.Contains("<div>Nothing to confirm</div>"))
                return Array.Empty<ConfirmationModel>();

            using var document = Parser.ParseDocument(response);
            List<ConfirmationModel> confirmationModels = new();

            foreach (var confirmationElement in document.GetElementsByClassName("mobileconf_list_entry"))
            {
                UInt64[] attributesValue = new UInt64[5];
                for (var i = 2; i <= 5; i++)
                    attributesValue[i - 2] = UInt64.Parse(confirmationElement.Attributes[i]!.Value);

                var confirmationElementContent = confirmationElement.FirstElementChild!;

                string imageSource = ((IHtmlImageElement)confirmationElementContent.QuerySelector("img")!).Source!;

                var children = confirmationElementContent.Children[confirmationElementContent.Children.Length - 1].Children;
                string[] descriptionArray = new string[3];
                for (var i = 0; i < 3; i++)
                {
                    var description = children[i];
                    descriptionArray[i] = description.TextContent;
                }

                confirmationModels.Add(new ConfirmationModel(attributesValue, imageSource, descriptionArray));
            }

            return confirmationModels;
        }

        private string? GenerateConfirmationHashForTime(Int64 time, string? tag)
        {
            byte[] decode = Convert.FromBase64String(IdentitySecret);
            int n2 = 8;

            if (tag is not null)
            {
                if (tag.Length > 32)
                {
                    n2 = 8 + 32;
                }
                else
                {
                    n2 = 8 + tag.Length;
                }
            }

            byte[] array = new byte[n2];
            int n3 = 8;

            while (true)
            {
                int n4 = n3 - 1;
                if (n3 <= 0)
                {
                    break;
                }
                array[n4] = (byte)time;
                time >>= 8;
                n3 = n4;
            }
            if (tag is not null)
                Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);

            try
            {
                using HMACSHA1 hmacGenerator = new()
                {
                    Key = decode
                };
                byte[] hashedData = hmacGenerator.ComputeHash(array);
                string encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
                return WebUtility.UrlEncode(encodedData);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
