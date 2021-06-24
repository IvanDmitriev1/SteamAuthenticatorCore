namespace SteamAuthCore.Models
{
    public static class ApiEndpoints
    {
        public const string SteamApiBase = "https://api.steampowered.com";
        public const string CommunityBase = "https://steamcommunity.com";
        public const string MobileauthBase = SteamApiBase + "/IMobileAuthService/%s/v0001";
        public static string MobileauthGetwgtoken = MobileauthBase.Replace("%s", "GetWGToken");
        public const string TwoFactorBase = SteamApiBase + "/ITwoFactorService/%s/v0001";
        public static string TwoFactorTimeQuery = TwoFactorBase.Replace("%s", "QueryTime");
    }
}
