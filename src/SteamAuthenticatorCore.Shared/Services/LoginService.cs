using System;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

internal sealed class LoginService : ILoginService
{
    public LoginService(AccountsFileServiceResolver accountsFileServiceResolver, IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService)
    {
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _platformImplementations = platformImplementations;
        _accountService = accountService;
    }

    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly ISteamGuardAccountService _accountService;

    public async Task RefreshLogin(SteamGuardAccount account, string password)
    {
        if (await RefreshSession(new LoginData(account.AccountName, password), account) is not { } session)
            return;

        account.Session = session;
        var manifestService = _accountsFileServiceResolver.Invoke();
        await manifestService.SaveAccount(account);

        await _platformImplementations.DisplayAlert("Session successfully refreshed");
    }

    private async Task<SessionData?> RefreshSession(LoginData loginData, SteamGuardAccount account)
    {
        var data = await _accountService.Login(loginData);

        /*while (true)
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
        }*/
    }
}