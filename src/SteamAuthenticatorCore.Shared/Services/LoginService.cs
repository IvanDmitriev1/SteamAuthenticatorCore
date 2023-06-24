using SteamAuthCore.Obsolete;

namespace SteamAuthenticatorCore.Shared.Services;

internal sealed class LoginService : ILoginService
{
    public LoginService(AccountsServiceResolver accountsServiceResolver, IPlatformImplementations platformImplementations, ISteamGuardAccountService steamGuardAccountService)
    {
        _accountsServiceResolver = accountsServiceResolver;
        _platformImplementations = platformImplementations;
        _steamGuardAccountService = steamGuardAccountService;
    }

    private readonly AccountsServiceResolver _accountsServiceResolver;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly ISteamGuardAccountService _steamGuardAccountService;

    public async Task<bool> RefreshLogin(SteamGuardAccount account, string password)
    {
        try
        {
            var result = await _steamGuardAccountService.LoginAgain(account, password, new LoginAgainData(), CancellationToken.None);
            return false;
            /*if (await RefreshSession(new UserLogin(account.AccountName, password), account) is not { } session)
                return false;

            account.Session = session;
            await _accountsServiceResolver.Invoke().Update(account);

            await _platformImplementations.DisplayAlert("Login",  "Session successfully refreshed");
            return true;*/
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);

            return false;
        }
    }

    private async Task<SessionData?> RefreshSession(UserLogin userLogin, SteamGuardAccount account)
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
                    userLogin.TwoFactorCode = account.GenerateSteamGuardCode();
                    continue;
                case LoginResult.BadRsa:
                    await _platformImplementations.DisplayAlert("Login", "Error logging in: Steam returned \"BadRSA\".");
                    return null;
                case LoginResult.BadCredentials:
                    await _platformImplementations.DisplayAlert("Login", "Error logging in: Username or password was incorrect.");
                    return null;
                case LoginResult.TooManyFailedLogins:
                    await _platformImplementations.DisplayAlert("Login", "Error logging in: Too many failed logins, try again later.");
                    return null;
                case LoginResult.GeneralFailure:
                    await _platformImplementations.DisplayAlert("Login", "Error logging in: Steam returned \"GeneralFailure\".");
                    return null;
                default:
                    await _platformImplementations.DisplayAlert("Login", "Error logging in: Steam returned \"GeneralFailure\".");
                    return null;
            }
        }
    }
}