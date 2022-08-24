namespace SteamAuthCore;

public static class ApiEndpoints
{
    public const string SteamApiBase = "https://api.steampowered.com";

    public const string MobileauthGetwgtoken = "/IMobileAuthService/GetWGToken/v0001";
    public const string TwoFactorTimeQuery = "/ITwoFactorService/QueryTime/v0001";


    public const string CommunityBase = "https://steamcommunity.com";

    public const string Mobileconf = "/mobileconf/";
    public const string MultipleConfirmations = Mobileconf + "multiajaxop";
}