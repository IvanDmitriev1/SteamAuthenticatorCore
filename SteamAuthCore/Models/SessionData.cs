using System;
using System.Net;

namespace SteamAuthCore.Models
{
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

        public string SessionId { get; set; }

        public string SteamLogin { get; set; }

        public string SteamLoginSecure { get; set; }

        public string WebCookie { get; set; }

        public string OAuthToken { get; set; }

        public UInt64 SteamId { get; set; }

        public void GetCookies(out CookieContainer cookies)
        {
            cookies = new CookieContainer();

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
        }
    }
}
