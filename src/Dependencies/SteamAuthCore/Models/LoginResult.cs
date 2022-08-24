namespace SteamAuthCore.Models;

public enum LoginResult
{
    GeneralFailure,
    LoginOkay,
    BadRsa,
    BadCredentials,
    NeedCaptcha,
    Need2Fa,
    NeedEmail,
    TooManyFailedLogins
}