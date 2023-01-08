using System.Text;

namespace SteamAuthCore.Models;

public class SessionData
{
    public SessionData(string sessionId, string steamLogin, string steamLoginSecure, string webCookie, string oAuthToken, ulong steamId)
    {
        SessionId = sessionId;
        SteamLogin = steamLogin;
        SteamLoginSecure = steamLoginSecure;
        WebCookie = webCookie;
        OAuthToken = oAuthToken;
        SteamId = steamId;
    }

    public string SessionId { get; }
    public string SteamLogin { get; set; }
    public string SteamLoginSecure { get; set; }
    public string WebCookie { get; }
    public string OAuthToken { get; }
    public ulong SteamId { get; }

    public string GetCookieString()
    {
        var builder = new StringBuilder(6);
        builder.Append($"steamid={SteamId};");
        builder.Append($"steamLogin={SteamLogin};");
        builder.Append($"steamLoginSecure={SteamLoginSecure};");
        builder.Append("Steam_Language=english;");
        builder.Append("dob= ;");
        builder.Append($"sessionid={SessionId};");

        return builder.ToString();
    }
}