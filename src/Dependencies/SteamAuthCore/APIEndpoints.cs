namespace SteamAuthCore;

public static class ApiEndpoints
{
    public const string SteamApiBase = "https://api.steampowered.com";

    public const string MobileauthGetwgtoken = "/IMobileAuthService/GetWGToken/v0001";
    public const string TwoFactorTimeQuery = "/ITwoFactorService/QueryTime/v0001";
    public const string RemoveAuthenticator = "/ITwoFactorService/RemoveAuthenticator/v0001";

    public const string CommunityBase = "https://steamcommunity.com";
    public const string MobileLoginRequestRefer = CommunityBase + "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";

    public const string Mobileconf = "/mobileconf/";
    public const string MultipleConfirmations = Mobileconf + "multiajaxop";
    public const string Login = "/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";
    public const string GetRsaKey = "/login/getrsakey";
    public const string DoLogin = "/login/dologin";
}