namespace SteamAuthCore;

public static class ApiEndpoints
{
    public const string SteamApiBase = "https://api.steampowered.com";

    public const string MobileauthGetwgtoken = "/IMobileAuthService/GetWGToken/v0001";
    public const string TwoFactorTimeQuery = "/ITwoFactorService/QueryTime/v0001";


    public const string CommunityBase = "https://steamcommunity.com";
    private const string MobileLoginRequestCons = "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";

    public const string Mobileconf = "/mobileconf/";
    public const string MultipleConfirmations = Mobileconf + "multiajaxop";
    public const string Login = "/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client" + MobileLoginRequestCons;
    public const string GetRsaKey = "/login/getrsakey" + MobileLoginRequestCons;
    public const string DoLogin = "/login/dologin" + MobileLoginRequestCons;
}