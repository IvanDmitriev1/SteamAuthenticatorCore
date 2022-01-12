using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamDesktopAuthenticatorCore.Views.Pages;
using WpfHelper.Commands;
using WPFUI.Controls;
using WPFUI.Controls.Navigation;
using BaseViewModel = WPFUI.Common.BaseViewModel;
using Icon = WPFUI.Common.Icon;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class LoginViewModel : BaseViewModel, INavigable
    {
        public LoginViewModel(App.ManifestServiceResolver manifestServiceResolver, DefaultNavigation navigation, Dialog dialog, Snackbar snackbar)
        {
            _manifestServiceResolver = manifestServiceResolver;
            _navigation = navigation;
            _dialog = dialog;
            _snackbar = snackbar;
        }

        #region Variables

        private readonly App.ManifestServiceResolver _manifestServiceResolver;
        private readonly DefaultNavigation _navigation;
        private readonly Dialog _dialog;
        private readonly Snackbar _snackbar;

        private SteamGuardAccount? _account;
        private UserLogin? _userLogin;
        private string _password = string.Empty;
        private string _userName = string.Empty;

        #endregion

        #region Public fileds

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public string UserName
        {
            get => _account is not null ? _account.AccountName : _userName;
            set => Set(ref _userName, value);
        }

        public bool IsEnabledUserNameTextBox => _account is null;

        #endregion

        #region Commands

        public ICommand LoginCommand => new AsyncRelayCommand(async o =>
        {
            if (_account is null)
            {
                await InitLogin();
                return;
            }

            await RefreshLogin();
        });

        #endregion

        #region Piblic methods

        public bool OnNavigationRequest(INavigation navigation, object[]? ars)
        {
            if (ars is null)
                return false;

            if (navigation.NavigationStack[^1].PageType == typeof(CaptchaPage))
            {
                _userLogin!.CaptchaText = (string?) ars[0];
                LoginCommand.Execute(null);
                return true;
            }

            _account = (SteamGuardAccount?) ars[0];
            OnPropertyChanged(nameof(IsEnabledUserNameTextBox), nameof(UserName));

            return true;
        }

        #endregion

        #region PrivateMethods

        private async Task RefreshLogin()
        {
            _userLogin ??= new UserLogin(UserName, Password);
            var steamTime = await TimeAligner.GetSteamTimeAsync();

            switch (_userLogin.DoLogin())
            {
                case LoginResult.NeedCaptcha:
                    _navigation.NavigateTo(nameof(CaptchaPage), new object[] { _userLogin.CaptchaGid ?? string.Empty });
                    return;
                case LoginResult.Need2Fa:
                    _userLogin.TwoFactorCode = _account!.GenerateSteamGuardCodeForTime(steamTime);
                    await RefreshLogin();
                    break;
                case LoginResult.BadRsa:
                    await _dialog.ShowDialog("Error logging in: Steam returned \"BadRSA\"", "Login Error");
                    return;
                case LoginResult.BadCredentials:
                    await _dialog.ShowDialog("Error logging in: Username or password was incorrect", "Login Error");
                    return;
                case LoginResult.TooManyFailedLogins:
                    await _dialog.ShowDialog("Error logging in: Too many failed logins, try again later", "Login Error");
                    return;
                case LoginResult.GeneralFailure:
                    await _dialog.ShowDialog("Error logging in: Steam returned \"GeneralFailure\".", "Login Error");
                    return;
                case LoginResult.LoginOkay:
                    _account!.Session = _userLogin.Session;

                    var manifestService = _manifestServiceResolver.Invoke();
                    await manifestService.SaveSteamGuardAccount(_account);
                    _snackbar.Expand("Login success", "Your login session was refreshed");
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task InitLogin()
        {

        }

        #endregion
    }
}
