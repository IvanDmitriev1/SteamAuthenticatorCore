using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using WPFUI.DIControls.Interfaces;
using AsyncRelayCommand = SteamAuthenticatorCore.Shared.Helpers.AsyncRelayCommand;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class LoginViewModel : INavigable
{
    public LoginViewModel(LoginService loginService, App.ManifestServiceResolver manifestServiceResolver, INavigation navigation, IDialog dialog, ISnackbar snackbar)
    {
        LoginService = loginService;
        _manifestServiceResolver = manifestServiceResolver;
        _navigation = navigation;
        _dialog = dialog;
        _snackbar = snackbar;

        LoginCommand = new AsyncRelayCommand(Login);
    }

    #region Variables

    private readonly App.ManifestServiceResolver _manifestServiceResolver;
    private readonly INavigation _navigation;
    private readonly IDialog _dialog;
    private readonly ISnackbar _snackbar;

    #endregion

    public LoginService LoginService { get; }

    public ICommand LoginCommand { get; }

    #region Public methods

    public void OnNavigationRequest(INavigation navigation, INavigationItem previousNavigationItem, ref object[]? ars)
    {
        if (ars is null)
            return;

        if (previousNavigationItem.PageType == typeof(CaptchaPage))
        {
            RefreshLoginAsync((string) ars[0]);
            return;
        }

        LoginService.Account = (SteamGuardAccount?) ars[0];
    }

    #endregion

    #region PrivateMethods

    private async Task Login()
    {
        if (LoginService.Account is null)
        {
            //await InitLogin();
            _navigation.NavigateTo($"{nameof(TokenPage)}");
            return;
        }

        await RefreshLogin();

        _navigation.NavigateTo($"{nameof(TokenPage)}");
    }


    private async Task RefreshLogin(string? captchaCode = null)
    {
        switch (await LoginService.RefreshLogin(captchaCode))
        {
            case LoginResult.LoginOkay:
                await _manifestServiceResolver.Invoke().SaveSteamGuardAccount(LoginService.Account!);
                _snackbar.Expand("Login success");
                break;
            case LoginResult.GeneralFailure:
                await _dialog.ShowDialog("Error logging in: Steam returned \"GeneralFailure\"");
                break;
            case LoginResult.BadRsa:
                await _dialog.ShowDialog("Error logging in: Steam returned \"BadRSA\"");
                break;
            case LoginResult.BadCredentials:
                await _dialog.ShowDialog("Error logging in: Username or password was incorrect");
                break;
            case LoginResult.TooManyFailedLogins:
                await _dialog.ShowDialog("Error logging in: Too many failed logins, try again later");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void RefreshLoginAsync(string captchaCode) => await RefreshLogin(captchaCode);

    #endregion
}