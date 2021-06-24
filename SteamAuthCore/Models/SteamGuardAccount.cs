using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamAuthCore.Models
{
    public class SteamGuardAccount
    {
        #region Exception

        public class WgTokenInvalidException : Exception
        {
        }

        public class WgTokenExpiredException : Exception
        {
        }

        #endregion

        #region HelpClasess

        private class RefreshSessionDataResponse
        {
            [JsonProperty("response")]
            public RefreshSessionDataInternalResponse? Response { get; set; }
            internal class RefreshSessionDataInternalResponse
            {
                [JsonProperty("token")]
                public string Token { get; set; } = null!;

                [JsonProperty("token_secure")]
                public string TokenSecure { get; set; } = null!;
            }
        }

        private class RemoveAuthenticatorResponse
        {
            [JsonProperty("response")]
            public RemoveAuthenticatorInternalResponse? Response { get; set; }

            internal class RemoveAuthenticatorInternalResponse
            {
                [JsonProperty("success")]
                public bool Success { get; set; }
            }
        }

        private class SendConfirmationResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
        }

        private class ConfirmationDetailsResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("html")]
            public string Html { get; set; } = null!;
        }

        #endregion

        #region Properties

        [JsonProperty("shared_secret")]
        public string? SharedSecret { get; set; }

        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; } = null!;

        [JsonProperty("revocation_code")]
        public string RevocationCode { get; set; } = null!;

        [JsonProperty("uri")]
        public string Uri { get; set; } = null!;

        [JsonProperty("server_time")]
        public long ServerTime { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; } = null!;

        [JsonProperty("token_gid")]
        public string TokenGid { get; set; } = null!;

        [JsonProperty("identity_secret")]
        public string IdentitySecret { get; set; } = null!;

        [JsonProperty("secret_1")]
        public string Secret1 { get; set; } = null!;

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; set; } = null!;

        /// <summary>
        /// Set to true if the authenticator has actually been applied to the account.
        /// </summary>
        [JsonProperty("fully_enrolled")]
        public bool FullyEnrolled { get; set; }

        public SessionData Session { get; set; } = null!;

        #endregion

        private static readonly byte[] SteamGuardCodeTranslations = { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };

        public bool DeactivateAuthenticator(int scheme = 2)
        {
            var postData = new NameValueCollection
            {
                {"steamid", Session.SteamId.ToString()},
                {"steamguard_scheme", scheme.ToString()},
                {"revocation_code", RevocationCode},
                {"access_token", Session.OAuthToken}
            };

            try
            {
                if (SteamWeb.MobileLoginRequest(ApiEndpoints.SteamApiBase + "/ITwoFactorService/RemoveAuthenticator/v0001", "POST", postData) is not { } response)
                    throw new ArgumentNullException(nameof(response));

                return JsonConvert.DeserializeObject<RemoveAuthenticatorResponse>(response) is {Response: {Success: true}};
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string? GenerateSteamGuardCode()
        {
            return GenerateSteamGuardCodeForTime(TimeAligner.GetSteamTime());
        }

        public string? GenerateSteamGuardCodeForTime(Int64 time)
        {
            if (string.IsNullOrEmpty(SharedSecret)) return "";

            string sharedSecretUnescaped = Regex.Unescape(SharedSecret);
            byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
            byte[] timeArray = new byte[8];

            time /= 30L;

            for (int i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            using HMACSHA1 hmacGenerator = new()
            {
                Key = sharedSecretArray
            };

            byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
            byte[] codeArray = new byte[5];
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
                return null; //Change later, catch-alls are bad!
            }
            return Encoding.UTF8.GetString(codeArray);
        }

        public ConfirmationModel[] FetchConfirmations()
        {
            string url = GenerateConfirmationUrl();

            Session.GetCookies(out var cookies);

            return FetchConfirmationInternal(SteamWeb.Request(url, "GET", "", cookies));
        }

        public async Task<ConfirmationModel[]> FetchConfirmationsAsync()
        {
            string url = GenerateConfirmationUrl();

            Session.GetCookies(out var cookies);

            return FetchConfirmationInternal(await SteamWeb.RequestAsync(url, "GET", null, cookies));
        }

        /// <summary>
        /// Deprecated. Simply returns conf.Creator.
        /// </summary>
        /// <param name="conf"></param>
        /// <returns>The Creator field of conf</returns>
        public Int64 GetConfirmationTradeOfferId(ConfirmationModel conf)
        {
            if (conf.ConfType != ConfirmationModel.ConfirmationType.Trade)
                throw new ArgumentException("conf must be a trade confirmation.");

            return (Int64)conf.Creator;
        }

        public bool AcceptMultipleConfirmations(ConfirmationModel[] confs)
        {
            return SendMultiConfirmationAjax(confs, "allow");
        }

        public bool DenyMultipleConfirmations(ConfirmationModel[] confs)
        {
            return SendMultiConfirmationAjax(confs, "cancel");
        }

        public bool AcceptConfirmation(ConfirmationModel conf)
        {
            return SendConfirmationAjax(conf, "allow");
        }

        public bool DenyConfirmation(ConfirmationModel conf)
        {
            return SendConfirmationAjax(conf, "cancel");
        }

        /// <summary>
        /// Refreshes the Steam session. Necessary to perform confirmations if your session has expired or changed.
        /// </summary>
        /// <returns></returns>
        public bool RefreshSession()
        {
            string url = ApiEndpoints.MobileauthGetwgtoken;
            NameValueCollection postData = new NameValueCollection();
            postData.Add("access_token", this.Session.OAuthToken);

            string response;
            try
            {
                string? request = SteamWeb.Request(url, "POST", postData);
                if (request is null)
                    return false;

                response = request;
            }
            catch (WebException)
            {
                return false;
            }

            try
            {
                if (JsonConvert.DeserializeObject<RefreshSessionDataResponse>(response) is not { } refreshResponse)
                    return false;

                if (refreshResponse.Response == null || string.IsNullOrEmpty(refreshResponse.Response.Token))
                    return false;

                string token = Session.SteamId + "%7C%7C" + refreshResponse.Response.Token;
                string tokenSecure = Session.SteamId + "%7C%7C" + refreshResponse.Response.TokenSecure;

                Session.SteamLogin = token;
                Session.SteamLoginSecure = tokenSecure;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Refreshes the Steam session. Necessary to perform confirmations if your session has expired or changed.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshSessionAsync()
        {
            string url = ApiEndpoints.MobileauthGetwgtoken;
            NameValueCollection postData = new NameValueCollection();
            postData.Add("access_token", this.Session.OAuthToken);

            string? response = null;
            try
            {
                response = await SteamWeb.RequestAsync(url, "POST", postData);
            }
            catch (WebException)
            {
                return false;
            }

            if (response is null) return false;

            try
            {
                RefreshSessionDataResponse? refreshResponse = JsonConvert.DeserializeObject<RefreshSessionDataResponse>(response);
                if (refreshResponse?.Response == null || string.IsNullOrEmpty(refreshResponse.Response.Token))
                    return false;

                string token = Session.SteamId + "%7C%7C" + refreshResponse.Response.Token;
                string tokenSecure = Session.SteamId + "%7C%7C" + refreshResponse.Response.TokenSecure;

                Session.SteamLogin = token;
                Session.SteamLoginSecure = tokenSecure;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GenerateConfirmationUrl(string tag = "conf")
        {
            string endpoint = ApiEndpoints.CommunityBase + "/mobileconf/conf?";
            string queryString = GenerateConfirmationQueryParams(tag);
            return endpoint + queryString;
        }

        public string GenerateConfirmationQueryParams(string tag)
        {
            if (String.IsNullOrEmpty(DeviceId))
                throw new ArgumentException("Device ID is not present");

            var queryParams = GenerateConfirmationQueryParamsAsNvc(tag);

            return "p=" + queryParams["p"] + "&a=" + queryParams["a"] + "&k=" + queryParams["k"] + "&t=" + queryParams["t"] + "&m=android&tag=" + queryParams["tag"];
        }

        public NameValueCollection GenerateConfirmationQueryParamsAsNvc(string tag)
        {
            if (String.IsNullOrEmpty(DeviceId))
                throw new ArgumentException("Device ID is not present");

            long time = TimeAligner.GetSteamTime();

            var ret = new NameValueCollection();
            ret.Add("p", this.DeviceId);
            ret.Add("a", this.Session.SteamId.ToString());
            ret.Add("k", GenerateConfirmationHashForTime(time, tag));
            ret.Add("t", time.ToString());
            ret.Add("m", "android");
            ret.Add("tag", tag);

            return ret;
        }

        #region PrivateMethods

        private static ConfirmationModel[] FetchConfirmationInternal(string? response)
        {

            /*So you're going to see this abomination and you're going to be upset.
              It's understandable. But the thing is, regex for HTML -- while awful -- makes this way faster than parsing a DOM, plus we don't need another library.
              And because the data is always in the same place and same format... It's not as if we're trying to naturally understand HTML here. Just extract strings.
              I'm sorry. */

            Regex confRegex = new("<div class=\"mobileconf_list_entry\" id=\"conf[0-9]+\" data-confid=\"(\\d+)\" data-key=\"(\\d+)\" data-type=\"(\\d+)\" data-creator=\"(\\d+)\"");

            if (response is null || !confRegex.IsMatch(response))
            {
                if (response is null || !response.Contains("<div>Nothing to confirm</div>"))
                    throw new WgTokenInvalidException();

                return new ConfirmationModel[0];
            }

            MatchCollection confirmations = confRegex.Matches(response);

            List<ConfirmationModel> ret = new();
            foreach (Match confirmation in confirmations)
            {
                if (confirmation.Groups.Count != 5) continue;

                if (!ulong.TryParse(confirmation.Groups[1].Value, out ulong confID) ||
                    !ulong.TryParse(confirmation.Groups[2].Value, out ulong confKey) ||
                    !int.TryParse(confirmation.Groups[3].Value, out int confType) ||
                    !ulong.TryParse(confirmation.Groups[4].Value, out ulong confCreator))
                {
                    continue;
                }

                ret.Add(new ConfirmationModel(confID, confKey, confType, confCreator));
            }

            return ret.ToArray();
        }

        private ConfirmationDetailsResponse? GetConfirmationDetails(ConfirmationModel conf)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/details/" + conf.Id + "?";
            string queryString = GenerateConfirmationQueryParams("details");
            url += queryString;

            Session.GetCookies(out var cookies);

            string? response = SteamWeb.Request(url, "GET", "", cookies, null);
            if (string.IsNullOrEmpty(response)) return null;

            return JsonConvert.DeserializeObject<ConfirmationDetailsResponse>(response) is not { } confResponse ? null : confResponse;
        }

        private bool SendConfirmationAjax(ConfirmationModel conf, string op)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/ajaxop";
            string queryString = "?op=" + op + "&";
            queryString += GenerateConfirmationQueryParams(op);
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;

            Session.GetCookies(out var cookies);

            if (SteamWeb.Request(url, "GET", "", cookies, null) is not { } response)
                return false;

            if (JsonConvert.DeserializeObject<SendConfirmationResponse>(response) is not { } confResponse)
                throw new ArgumentNullException(nameof(confResponse));

            return confResponse.Success;
        }

        private bool SendMultiConfirmationAjax(ConfirmationModel[] confs, string op)
        {
            string url = ApiEndpoints.CommunityBase + "/mobileconf/multiajaxop";

            string query = "op=" + op + "&" + GenerateConfirmationQueryParams(op);
            foreach (var conf in confs)
            {
                query += "&cid[]=" + conf.Id + "&ck[]=" + conf.Key;
            }

            Session.GetCookies(out var cookies);

            if (SteamWeb.Request(url, "POST", query, cookies, null) is not { } response)
                return false;

            if (JsonConvert.DeserializeObject<SendConfirmationResponse>(response) is not { } confResponse)
                throw new ArgumentNullException(nameof(confResponse));

            return confResponse.Success;
        }

        private string? GenerateConfirmationHashForTime(long time, string? tag)
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
