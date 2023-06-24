namespace SteamAuthCore.Models;

public class SessionData
{
    public string? SessionId { get; init; }
    public string? SteamLoginSecure { get; set; }
    public string? AccessToken { get; init; }
    public string? WebCookie { get; init; }
    public ulong SteamId { get; init; }

    public string GetCookieString()
    {
        var builder = new StringBuilder(6);
        builder.Append($"steamid={SteamId};");
        builder.Append($"steamLoginSecure={SteamLoginSecure};");
        builder.Append("Steam_Language=english;");
        builder.Append("dob= ;");
        builder.Append($"sessionid={SessionId};");

        return builder.ToString();
    }

    public CookieContainer GetCookieContainer()
    {
        CookieContainer cookies = new();

        cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
        cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));

        cookies.Add(new Cookie("steamid", SteamId.ToString(), "/", ".steamcommunity.com"));

        cookies.Add(new Cookie("steamLoginSecure", SteamLoginSecure, "/", ".steamcommunity.com")
        {
            HttpOnly = true,
            Secure = true
        });

        cookies.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
        cookies.Add(new Cookie("dob", "", "/", ".steamcommunity.com"));
        cookies.Add(new Cookie("sessionid", SessionId, "/", ".steamcommunity.com"));

        return cookies;
    }
}