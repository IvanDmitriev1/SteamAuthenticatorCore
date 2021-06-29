using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SteamAuthCore.Models;

namespace SteamAuthCore
{
    public enum LoginResult
    {
        LoginOkay,
        GeneralFailure,
        BadRsa,
        BadCredentials,
        NeedCaptcha,
        Need2Fa,
        NeedEmail,
        TooManyFailedLogins
    }

    public class UserLogin
    {
        public UserLogin(string username, string password)
        {
            Username = username;
            Password = password;

            _cookies = new CookieContainer();
        }

        #region HelpClasses

        private class LoginResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("login_complete")]
            public bool LoginComplete { get; set; }

            [JsonProperty("oauth")]
            public string? OAuthDataString { get; set; }

            public OAuth? OAuthData => OAuthDataString != null ? JsonConvert.DeserializeObject<OAuth>(OAuthDataString) : null;

            [JsonProperty("captcha_needed")]
            public bool CaptchaNeeded { get; set; }

            [JsonProperty("captcha_gid")]
            public string CaptchaGid { get; set; } = null!;

            [JsonProperty("emailsteamid")]
            public ulong EmailSteamID { get; set; }

            [JsonProperty("emailauth_needed")]
            public bool EmailAuthNeeded { get; set; }

            [JsonProperty("requires_twofactor")]
            public bool TwoFactorNeeded { get; set; }

            [JsonProperty("message")]
            public string? Message { get; set; }

            internal class OAuth
            {
                [JsonProperty("steamid")]
                public ulong SteamId { get; set; }

                [JsonProperty("oauth_token")]
                public string? OAuthToken { get; set; }

                [JsonProperty("wgtoken")]
                public string SteamLogin { get; set; } = null!;

                [JsonProperty("wgtoken_secure")]
                public string SteamLoginSecure { get; set; } = null!;

                [JsonProperty("webcookie")]
                public string Webcookie { get; set; } = null!;
            }
        }

        private class RsaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("publickey_exp")]
            public string Exponent { get; set; } = null!;

            [JsonProperty("publickey_mod")]
            public string Modulus { get; set; } = null!;

            [JsonProperty("timestamp")]
            public string Timestamp { get; set; } = null!;

            [JsonProperty("steamid")]
            public ulong SteamId { get; set; }
        }

        #endregion

        #region Fiedls

        public string Username { get; }
        public string Password { get; }
        public UInt64 SteamId { get; private set; }

        public bool RequiresCaptcha { get; private set; }
        public string? CaptchaGid { get; private set; }
        public string? CaptchaText { get; set; } = null;

        public bool RequiresEmail { get; private set; }
        public string? EmailDomain { get; } = null;
        public string? EmailCode { get; set; } = null;

        public bool Requires2Fa { get; private set; }

        public string? TwoFactorCode { get; set; } = null;

        public SessionData Session { get; private set; } = null!;
        public bool LoggedIn { get; private set; }

        #endregion

        #region Variables

        private readonly CookieContainer _cookies;

        #endregion

        public LoginResult DoLogin()
        {
            NameValueCollection postData = new();
            CookieContainer cookies = _cookies;

            if (cookies.Count == 0)
            {
                //Generate a SessionID
                cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
                cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));
                cookies.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));

                NameValueCollection headers = new NameValueCollection();
                headers.Add("X-Requested-With", "com.valvesoftware.android.steam.community");

                SteamWeb.MobileLoginRequest("https://steamcommunity.com/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client", "GET", null, cookies, headers);
            }

            postData.Add("donotcache", (TimeAligner.GetSteamTime() * 1000).ToString());
            postData.Add("username", Username);


            if (SteamWeb.MobileLoginRequest(ApiEndpoints.CommunityBase + "/login/getrsakey", "POST", postData, cookies) is not { } response)
                return LoginResult.GeneralFailure;

            if (response.Contains("<BODY>\nAn error occurred while processing your request."))
                return LoginResult.GeneralFailure;

            if (JsonConvert.DeserializeObject<RsaResponse>(response) is not { } rsaResponse)
                throw new ArgumentNullException(nameof(rsaResponse));

            if (!rsaResponse.Success)
                return LoginResult.BadRsa;

            Thread.Sleep(350); //Sleep for a bit to give Steam a chance to catch up??

            byte[] encryptedPasswordBytes;
            using (var rsaEncryptor = new RSACryptoServiceProvider())
            {
                var passwordBytes = Encoding.ASCII.GetBytes(this.Password);
                var rsaParameters = rsaEncryptor.ExportParameters(false);
                rsaParameters.Exponent = Util.HexStringToByteArray(rsaResponse.Exponent);
                rsaParameters.Modulus = Util.HexStringToByteArray(rsaResponse.Modulus);
                rsaEncryptor.ImportParameters(rsaParameters);
                encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);
            }

            string encryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

            postData.Clear();

            postData.Add("donotcache", (TimeAligner.GetSteamTime() * 1000).ToString());

            postData.Add("password", encryptedPassword);
            postData.Add("username", Username);
            postData.Add("twofactorcode", TwoFactorCode ?? "");

            postData.Add("emailauth", RequiresEmail ? EmailCode : "");
            postData.Add("loginfriendlyname", "");
            postData.Add("captchagid", RequiresCaptcha ? CaptchaGid : "-1");
            postData.Add("captcha_text", RequiresCaptcha ? CaptchaText : "");
            postData.Add("emailsteamid", (Requires2Fa || RequiresEmail) ? SteamId.ToString() : "");

            postData.Add("rsatimestamp", rsaResponse.Timestamp);
            postData.Add("remember_login", "true");
            postData.Add("oauth_client_id", "DE45CD61");
            postData.Add("oauth_scope", "read_profile write_profile read_client write_client");

            if (SteamWeb.MobileLoginRequest(ApiEndpoints.CommunityBase + "/login/dologin", "POST", postData, cookies) is not { } newResponse)
                return LoginResult.GeneralFailure;

            if (JsonConvert.DeserializeObject<LoginResponse>(newResponse) is not {} loginResponse)
                throw new ArgumentNullException(nameof(loginResponse));

            if (loginResponse.Message is not null)
            {
                if (loginResponse.Message.Contains("There have been too many login failures"))
                    return LoginResult.TooManyFailedLogins;

                if (loginResponse.Message.Contains("Incorrect login"))
                    return LoginResult.BadCredentials;
            }

            if (loginResponse.CaptchaNeeded)
            {
                RequiresCaptcha = true;
                CaptchaGid = loginResponse.CaptchaGid;
                return LoginResult.NeedCaptcha;
            }

            if (loginResponse.EmailAuthNeeded)
            {
                RequiresEmail = true;
                SteamId = loginResponse.EmailSteamID;
                return LoginResult.NeedEmail;
            }

            if (loginResponse.TwoFactorNeeded && !loginResponse.Success)
            {
                this.Requires2Fa = true;
                return LoginResult.Need2Fa;
            }

            if (loginResponse.OAuthData?.OAuthToken is null || loginResponse.OAuthData.OAuthToken.Length == 0)
                return LoginResult.GeneralFailure;

            if (!loginResponse.LoginComplete)
                return LoginResult.BadCredentials;

            CookieCollection readableCookies = cookies.GetCookies(new Uri("https://steamcommunity.com"));
            var oAuthData = loginResponse.OAuthData;

            Session = new SessionData(
                readableCookies["sessionid"]!.Value,
                oAuthData.SteamId + "%7C%7C" + oAuthData.SteamLogin,
                oAuthData.SteamId + "%7C%7C" + oAuthData.SteamLogin,
                oAuthData.Webcookie,
                oAuthData.OAuthToken,
                oAuthData.SteamId);

            LoggedIn = true;
            return LoginResult.LoginOkay;
        }
    }
}
