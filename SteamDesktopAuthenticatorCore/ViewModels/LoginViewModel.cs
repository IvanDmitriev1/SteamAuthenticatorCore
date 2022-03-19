using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using WPFUI.DIControls.Interfaces;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject, INavigable
{
    public LoginViewModel(App.ManifestServiceResolver manifestServiceResolver, INavigation navigation, IDialog dialog, ISnackbar snackbar)
    {
        _manifestServiceResolver = manifestServiceResolver;
        _navigation = navigation;
        _dialog = dialog;
        _snackbar = snackbar;
    }

    #region Variables

    private readonly App.ManifestServiceResolver _manifestServiceResolver;
    private readonly INavigation _navigation;
    private readonly IDialog _dialog;
    private readonly ISnackbar _snackbar;

    private SteamGuardAccount? _account;
    private UserLogin? _userLogin;
        
    [ObservableProperty]
    private string _password = string.Empty;

    private string _userName = string.Empty;

    #endregion

    #region Public fileds

    public string UserName
    {
        get => _account is not null ? _account.AccountName : _userName;
        set
        {
            SetProperty(ref _userName, value);

            OnPropertyChanged(nameof(IsEnabledUserNameTextBox));
            OnPropertyChanged(nameof(UserName));
        }
    }

    public bool IsEnabledUserNameTextBox => _account is null;

    #endregion

    #region Commands

    [ICommand]
    private async Task Login()
    {
        if (_account is null)
        {
            await InitLogin();
            _navigation.NavigateTo($"{nameof(TokenPage)}");
            return;
        }

        await RefreshLogin();
        _navigation.NavigateTo($"{nameof(TokenPage)}");
    }

    #endregion

    #region Public methods

    public void OnNavigationRequest(INavigation navigation, INavigationItem previousNavigationItem, ref object[]? ars)
    {
        if (ars is null)
            return;

        if (previousNavigationItem.PageType == typeof(CaptchaPage))
        {
            _userLogin!.CaptchaText = (string?) ars[0];
            LoginAsync();
        }

        _account = (SteamGuardAccount?) ars[0];
    }

    #endregion

    #region PrivateMethods

    private async void LoginAsync() => await Login();

    private async Task RefreshLogin()
    {
        while (true)
        {
            _userLogin ??= new UserLogin(UserName, Password);
            var steamTime = await TimeAligner.GetSteamTimeAsync();

            switch (_userLogin.DoLogin())
            {
                case LoginResult.NeedCaptcha:
                    _navigation.NavigateTo($"//{nameof(CaptchaPage)}", new object[] {_userLogin.CaptchaGid ?? string.Empty});
                    return;
                case LoginResult.Need2Fa:
                    _userLogin.TwoFactorCode = _account!.GenerateSteamGuardCode(steamTime);
                    continue;
                case LoginResult.BadRsa:
                    await _dialog.ShowDialog("Error logging in: Steam returned \"BadRSA\"");
                    return;
                case LoginResult.BadCredentials:
                    await _dialog.ShowDialog("Error logging in: Username or password was incorrect");
                    return;
                case LoginResult.TooManyFailedLogins:
                    await _dialog.ShowDialog("Error logging in: Too many failed logins, try again later");
                    return;
                case LoginResult.GeneralFailure:
                    await _dialog.ShowDialog("Error logging in: Steam returned \"GeneralFailure\".");
                    return;
                case LoginResult.LoginOkay:
                    _account!.Session = _userLogin.Session;

                    var manifestService = _manifestServiceResolver.Invoke();
                    await manifestService.SaveSteamGuardAccount(_account);
                    _snackbar.Expand("Login success");
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private async Task InitLogin()
    {

    }

    #endregion
}