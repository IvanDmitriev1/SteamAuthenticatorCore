namespace SteamAuthCore;

public static class ApiEndpoints
{
    public const string SteamApiBase = "https://api.steampowered.com";
    public const string CommunityBase = "https://steamcommunity.com";
    public const string LoginSteampoweredBase = "https://login.steampowered.com";
    public const string LoginOauth = "oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";

    public const string MobileLoginRefer = $"{CommunityBase}/mobilelogin?{LoginOauth}";

    public const string MobileauthGetwgtoken = "/IMobileAuthService/GetWGToken/v0001";
    public const string TwoFactorTimeQuery = "/ITwoFactorService/QueryTime/v0001";
    public const string RemoveAuthenticator = "/ITwoFactorService/RemoveAuthenticator/v0001";

    public const string LoginGetRSAKey = "login/getrsakey";
    public const string DoLogin = "login/dologin";

    public const string Mobileconf = "/mobileconf/getlist?";
    public const string MultipleConfirmations = "mobileconf/multiajaxop";
    public const string SingleConfirmations = "mobileconf/ajaxop";

    public const string GetPasswordRsaPublicKey = SteamApiBase + "/IAuthenticationService/GetPasswordRSAPublicKey/v1/";
    public const string BeginAuthSessionViaCredentials = SteamApiBase + "/IAuthenticationService/BeginAuthSessionViaCredentials/v1/";
    public const string UpdateAuthSessionWithSteamGuardCode = SteamApiBase + "/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1/";
    public const string PollAuthSessionStatus = SteamApiBase + "/IAuthenticationService/PollAuthSessionStatus/v1/";

    public const string Finalizelogin = LoginSteampoweredBase + "/jwt/finalizelogin";
    public const string Settoken = CommunityBase + "/login/settoken";
}