using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared;

public partial class LoginService : ObservableObject
{
    public LoginService()
    {
        _password = string.Empty;
        _username = string.Empty;
    }

    private SteamGuardAccount? _account;
    

    #region Properties

    public SteamGuardAccount? Account
    {
        get => _account;
        set
        {
            SetProperty(ref _account, value);
            Password = string.Empty;

            if (value is not null)
                Username = value.AccountName;
        }
    }
    

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _username;

    public bool IsEnabledUserNameTextBox => _account is null;

    #endregion

    public async Task<LoginResult> RefreshLogin(string? twoFactorCode = null, string? captcha = null)
    {
        UserLogin userLogin = new(Account!.AccountName, Password);

        if (twoFactorCode is not null)
            userLogin.TwoFactorCode = twoFactorCode;

        if (captcha is not null)
            userLogin.CaptchaText = captcha;

        LoginResult result;
        switch (result = await userLogin.DoLogin())
        {
            case LoginResult.NeedCaptcha:
                return result;
            case LoginResult.Need2Fa:
                var code = Account.GenerateSteamGuardCode(await TimeAligner.GetSteamTimeAsync());
                return await RefreshLogin(code);
            case LoginResult.GeneralFailure:
                return result;
            case LoginResult.BadRsa:
                return result;
            case LoginResult.BadCredentials:
                return result;
            case LoginResult.TooManyFailedLogins:
                return result;
            case LoginResult.LoginOkay:
                Account.Session = userLogin.Session;
                return result;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task InitLogin()
    {

    }
}