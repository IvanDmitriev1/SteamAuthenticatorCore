namespace SteamAuthCore.Models;

public class SessionData
{
    public required string? SessionId { get; init; }
    public required string? SteamLoginSecure { get; init; }
    public required UInt64 SteamId { get; init; }

    public string? AccessToken { get; init; }
    public string? WebCookie { get; init; }

    [JsonIgnore]
    public string CookieString => _cookieString ??= GenerateCookie();

    private string? _cookieString;

    private string GenerateCookie()
    {
        var builder = new StringBuilder(200);
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