using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Obsolete;

public class UserLogin
{
    public UserLogin(string username, string password)
    {
        Username = username;
        Password = password;

        _cookies = new CookieContainer();
    }

    #region Fiedls

    public string Username { get; }
    public string Password { get; }
    public UInt64 SteamId { get; private set; }
    public bool RequiresCaptcha { get; private set; }
    public string? CaptchaGid { get; private set; }
    public string? CaptchaText { get; set; } = null;
    public bool RequiresEmail { get; private set; }
    public string? EmailCode { get; set; } = null;
    public bool Requires2Fa { get; private set; }
    public string? TwoFactorCode { get; set; }
    public SessionData? Session { get; private set; }
    public bool LoggedIn { get; private set; }
    public string? RefreshToken { get; private set; }

    #endregion

    #region Variables

    private readonly CookieContainer _cookies;

    #endregion

    public async Task<LoginResult> DoLoginV2()
    {
        NameValueCollection postData = new();
        CookieContainer cookies = _cookies;

        if (cookies.Count == 0)
        {
            //Generate a SessionID

            await SteamApi.RequestAsync("https://steamcommunity.com/", SteamApi.RequestMethod.Get, string.Empty, cookies);
            await SteamApi.RequestAsync("https://store.steampowered.com/", SteamApi.RequestMethod.Get, string.Empty, cookies);
        }

        var c1 = cookies.GetCookies(new Uri("https://steamcommunity.com"));
        if (c1["sessionid"]?.Value is null)
            return LoginResult.GeneralFailure;


        if (await SteamApi.RequestAsync<GetPasswordRsaPublicKey>(ApiEndpoints.GetPasswordRsaPublicKey + "?account_name=" + Username, SteamApi.RequestMethod.Get, postData, cookies) is not { } rsaResponse)
            return LoginResult.BadRsa;

        await Task.Delay(350); //Sleep for a bit to give Steam a chance to catch up??


        byte[] encryptedPasswordBytes;
        using (var rsaEncryptor = new RSACryptoServiceProvider())
        {
            var passwordBytes = Encoding.ASCII.GetBytes(this.Password);
            var rsaParameters = rsaEncryptor.ExportParameters(false);
            rsaParameters.Exponent = Util.HexStringToByteArray(rsaResponse.Response.PublickeyExp);
            rsaParameters.Modulus = Util.HexStringToByteArray(rsaResponse.Response.PublickeyMod);
            rsaEncryptor.ImportParameters(rsaParameters);
            encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);
        }

        string encryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

        //-------------------------------------

        postData.Clear();
        postData.Add("persistence", "1");
        postData.Add("encrypted_password", encryptedPassword);
        postData.Add("account_name", this.Username);
        postData.Add("encryption_timestamp", rsaResponse.Response.Timestamp);

        if (await SteamApi.RequestAsync<BeginAuthSessionViaCredentials>(ApiEndpoints.BeginAuthSessionViaCredentials, SteamApi.RequestMethod.Post, postData, cookies) is not { } loginResponse)
            return LoginResult.BadCredentials;

        if (loginResponse.Response.AllowedConfirmations?.FirstOrDefault(confirmation => confirmation.ConfirmationType == 3) is not null)
        {
            Requires2Fa = true;
            return LoginResult.Need2Fa;
        }

        if (loginResponse.Response.AllowedConfirmations?.FirstOrDefault(confirmation => confirmation.ConfirmationType == 2) is not null)
        {
            RequiresEmail = true;
            SteamId = UInt64.Parse(loginResponse.Response.Steamid);
            return LoginResult.NeedEmail;
        }

        if (!string.IsNullOrEmpty(EmailCode))
        {
            postData.Clear();
            postData.Add("client_id", loginResponse.Response.ClientId);
            postData.Add("steamid", loginResponse.Response.Steamid);
            postData.Add("code_type", "2");
            postData.Add("code", EmailCode);

            if (await SteamApi.MobileLoginRequest(ApiEndpoints.UpdateAuthSessionWithSteamGuardCode, SteamApi.RequestMethod.Post, postData, cookies) is null)
                return LoginResult.GeneralFailure;
        }

        postData.Clear();
        postData.Add("client_id", loginResponse.Response.ClientId);
        postData.Add("request_id", loginResponse.Response.RequestId);

        if (await SteamApi.RequestAsync<PollAuthSessionStatus>(ApiEndpoints.PollAuthSessionStatus, SteamApi.RequestMethod.Post, postData, cookies) is not { } pollAuthSessionStatus)
            return LoginResult.GeneralFailure;

        if (string.IsNullOrEmpty(pollAuthSessionStatus.Response.AccessToken) || string.IsNullOrEmpty(pollAuthSessionStatus.Response.RefreshToken))
            return LoginResult.BadCredentials;


        //-------------------------------------
        postData.Clear();
        postData.Add("nonce", pollAuthSessionStatus.Response.RefreshToken);
        postData.Add("sessionid", cookies.GetCookies(new Uri("https://steamcommunity.com"))["sessionid"]!.Value);
        postData.Add("redir", "https://steamcommunity.com/login/home/?goto=");

        if (await SteamApi.RequestAsync<Finalizelogin>(ApiEndpoints.Finalizelogin, SteamApi.RequestMethod.Post, postData, cookies) is not { } finalizelogin)
            return LoginResult.GeneralFailure;

        if (finalizelogin.TransferInfo is null || finalizelogin.TransferInfo.FirstOrDefault(data => data.Url == "https://steamcommunity.com/login/settoken") is null || string.IsNullOrEmpty(finalizelogin.SteamId))
            return LoginResult.GeneralFailure;


        //-------------------------------------
        postData.Clear();
        postData.Add("nonce", finalizelogin.TransferInfo.FirstOrDefault(x => x.Url == "https://steamcommunity.com/login/settoken")!.Params.Nonce);
        postData.Add("auth", finalizelogin.TransferInfo.FirstOrDefault(x => x.Url == "https://steamcommunity.com/login/settoken")!.Params.Auth);
        postData.Add("steamID", loginResponse.Response.Steamid);

        if (await SteamApi.RequestAsync(ApiEndpoints.Settoken, SteamApi.RequestMethod.Post, postData, cookies) is null)
            return LoginResult.GeneralFailure;

        if (await SteamApi.RequestAsync("https://store.steampowered.com/login/settoken", SteamApi.RequestMethod.Post, postData, cookies) is null)
            return LoginResult.GeneralFailure;

        var readableCookies = cookies.GetCookies(new Uri("https://steamcommunity.com"));

        if (readableCookies["steamLoginSecure"]?.Value == null)
            return LoginResult.GeneralFailure;


        RefreshToken = pollAuthSessionStatus.Response.RefreshToken;

        Session = new SessionData()
        {
            AccessToken = pollAuthSessionStatus.Response.AccessToken,
            SteamId = UInt64.Parse(loginResponse.Response.Steamid),
            SteamLoginSecure = readableCookies["steamLoginSecure"]!.Value,
            SessionId = readableCookies["sessionid"]!.Value
        };
        LoggedIn = true;
        return LoginResult.LoginOkay;
    }

    public async Task<LoginResult> DoLogin()
    {
        var cookies = _cookies;
        
        if (cookies.Count == 0)
        {
            //Generate a SessionID
            cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));

            NameValueCollection headers = new NameValueCollection
            {
                {
                    "X-Requested-With", "com.valvesoftware.android.steam.community"
                }
            };

            await SteamApi.MobileLoginRequest(
                "https://steamcommunity.com/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client",
                SteamApi.RequestMethod.Get, null, cookies, headers);
        }


        NameValueCollection postData = new();
        postData.Add("donotcache", (ITimeAligner.SteamTime * 1000).ToString());
        postData.Add("username", this.Username);

        if (await SteamApi.MobileLoginRequest<RsaResponse>(ApiEndpoints.CommunityBase + "/login/getrsakey",
                SteamApi.RequestMethod.Post, postData, cookies) is not { Success: true } rsaResponse)
            return LoginResult.GeneralFailure;

        await Task.Delay(350); //Sleep for a bit to give Steam a chance to catch up??

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

        postData.Add("donotcache", (ITimeAligner.SteamTime * 1000).ToString());

        postData.Add("password", encryptedPassword);
        postData.Add("username", Username);
        postData.Add("twofactorcode", TwoFactorCode ?? string.Empty);

        postData.Add("emailauth", RequiresEmail ? EmailCode : string.Empty);
        postData.Add("captchagid", RequiresCaptcha ? CaptchaGid : "-1");
        postData.Add("captcha_text", RequiresCaptcha ? CaptchaText : string.Empty);
        postData.Add("emailsteamid", (Requires2Fa || RequiresEmail) ? SteamId.ToString() : string.Empty);

        postData.Add("rsatimestamp", rsaResponse.Timestamp);
        postData.Add("remember_login", "true");

        if (await SteamApi.MobileLoginRequest<DoLoginResult>(ApiEndpoints.CommunityBase + "/login/dologin",
                SteamApi.RequestMethod.Post, postData, cookies) is not {  } loginResponse)
            return LoginResult.GeneralFailure;

        if (loginResponse.Message is not null && loginResponse.Message.Contains("There have been too many login failures"))
            return LoginResult.TooManyFailedLogins;

        if (loginResponse.Message is not null && loginResponse.Message.Contains("Incorrect login"))
            return LoginResult.BadCredentials;

        if (loginResponse.CaptchaNeeded)
        {
            RequiresCaptcha = true;
            CaptchaGid = loginResponse.CaptchaGid?.Deserialize<string>();
            return LoginResult.NeedCaptcha;
        }

        if (loginResponse.EmailAuthNeeded)
        {
            RequiresEmail = true;
            SteamId = loginResponse.EmailSteamId;
            return LoginResult.NeedEmail;
        }

        if (loginResponse is { TwoFactorNeeded: true, Success: false })
        {
            Requires2Fa = true;
            return LoginResult.Need2Fa;
        }

        if (loginResponse is { Success: true, TransferParameters: not null })
        {
            var readableCookies = cookies.GetCookies(new Uri("https://steamcommunity.com"));
            var transferParameters = loginResponse.TransferParameters;
            
            Session = new SessionData()
            {
                WebCookie = transferParameters.Webcookie,
                SteamId = UInt64.Parse(transferParameters.Steamid),
                SteamLoginSecure = readableCookies["steamLoginSecure"]!.Value,
                SessionId = readableCookies["sessionid"]!.Value,
            };
            LoggedIn = true;
            return LoginResult.LoginOkay;
        }

        return LoginResult.GeneralFailure;
    }
}