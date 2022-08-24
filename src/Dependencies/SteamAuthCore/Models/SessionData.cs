using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using SteamAuthCore.Exceptions;

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

    public CookieContainer GetCookies()
    {
        try
        {
            CookieContainer cookies = new();

            cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));

            cookies.Add(new Cookie("steamid", SteamId.ToString(), "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("steamLogin", SteamLogin, "/", ".steamcommunity.com")
            {
                HttpOnly = true
            });

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
        catch (CookieException)
        {
            throw new WgTokenExpiredException();
        }
    }

    public KeyValuePair<string, string>[] GetKeyValuePairCookies()
    {
        try
        {
            var cookies = new KeyValuePair<string, string>[8];
            cookies[0] = new KeyValuePair<string, string>("mobileClientVersion", "0 (2.1.3)");
            cookies[1] = new KeyValuePair<string, string>("mobileClient", "android");
            cookies[2] = new KeyValuePair<string, string>("steamid", SteamId.ToString());
            cookies[3] = new KeyValuePair<string, string>("steamLogin", SteamLogin);
            cookies[4] = new KeyValuePair<string, string>("steamLoginSecure", SteamLoginSecure);
            cookies[5] = new KeyValuePair<string, string>("Steam_Language", "english");
            cookies[6] = new KeyValuePair<string, string>("dob", "");
            cookies[7] = new KeyValuePair<string, string>("sessionid", SessionId);

            return cookies;
        }
        catch (CookieException)
        {
            throw new WgTokenExpiredException();
        }
    }
}