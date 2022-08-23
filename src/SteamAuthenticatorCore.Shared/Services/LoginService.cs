using System;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.Services;

public sealed class LoginService : ILoginService
{
    public LoginService(AccountsFileServiceResolver accountsFileServiceResolver, IPlatformImplementations platformImplementations)
    {
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _platformImplementations = platformImplementations;
    }

    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly IPlatformImplementations _platformImplementations;

    public async Task RefreshLogin(SteamGuardAccount account, string password)
    {
        if (await RefreshSession(new UserLogin(account.AccountName, password), account) is not { } session)
            return;

        account.Session = session;
        var manifestService = _accountsFileServiceResolver.Invoke();
        await manifestService.SaveAccount(account);

        await _platformImplementations.DisplayAlert("Session successfully refreshed");
    }

    private async ValueTask<SessionData?> RefreshSession(UserLogin userLogin, SteamGuardAccount account)
    {
        while (true)
        {
            switch (await userLogin.DoLogin())
            {
                case LoginResult.LoginOkay:
                    return userLogin.Session;
                case LoginResult.NeedCaptcha:
                    throw new NotImplementedException($"{LoginResult.NeedCaptcha} not implemented");
                case LoginResult.Need2Fa:
                    userLogin.TwoFactorCode = account.GenerateSteamGuardCode(await TimeAligner.GetSteamTimeAsync());
                    continue;
                case LoginResult.BadRsa:
                    await _platformImplementations.DisplayAlert("Error logging in: Steam returned \"BadRSA\".");
                    return null;
                case LoginResult.BadCredentials:
                    await _platformImplementations.DisplayAlert("Error logging in: Username or password was incorrect.");
                    return null;
                case LoginResult.TooManyFailedLogins:
                    await _platformImplementations.DisplayAlert("Error logging in: Too many failed logins, try again later.");
                    return null;
                case LoginResult.GeneralFailure:
                    await _platformImplementations.DisplayAlert("Error logging in: Steam returned \"GeneralFailure\".");
                    return null;
                default:
                    return null;
            }
        }
    }
}