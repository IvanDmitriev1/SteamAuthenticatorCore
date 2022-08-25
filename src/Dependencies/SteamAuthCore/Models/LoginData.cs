namespace SteamAuthCore.Models;

public class LoginData
{
    public LoginData(string username, string password)
    {
        Username = username;
        Password = password;
        TwoFactorCode = string.Empty;
    }

    public string Username { get; }
    public string Password { get; }
    public string TwoFactorCode { get; set; }

    public LoginResult Result { get; internal set; }
}
