using System;
using System.Text;

namespace SteamAuthCore.Models;

public class LoginData
{
    public LoginData(string username, string password)
    {
        Username = username;
        Password = password;
        TwoFactorCode = string.Empty;
        SessionId = string.Empty;
        CaptchaText = string.Empty;
        CookieString = string.Empty;
    }

    static LoginData()
    {
        var cookieBuilder = new StringBuilder(3);
        cookieBuilder.Append("mobileClientVersion=0 (2.1.3);");
        cookieBuilder.Append("mobileClient=android;");
        cookieBuilder.Append("Steam_Language=english;");

        DefaultCookies = cookieBuilder.ToString();
    }

    public string Username { get; }
    public string Password { get; }
    public string TwoFactorCode { get; set; }
    public string CaptchaText { get; set; }
    public string? CaptchaGid { get; internal set; }
    public string SessionId { get; internal set; }
    public UInt64 SteamId { get; internal set; }
    public string CookieString { get; internal set; }
    public SessionData? SessionData { get; internal set; }
    public LoginResult Result { get; internal set; }

    internal static string DefaultCookies { get; }
}
