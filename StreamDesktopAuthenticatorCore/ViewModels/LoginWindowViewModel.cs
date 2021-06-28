using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class LoginWindowViewModel : BaseViewModel
    {
        public LoginWindowViewModel()
        {
            _loginExplanation = "This will activate Steam Desktop Authenticator on your Steam account. This requires a phone number that can receive SMS.";
        }

        #region variabrls

        private Window? _thisWindow;
        private LoginType _loginType;
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _loginExplanation;
        private bool _userNameEnabled = true;

        #endregion

        #region Fields

        public SteamGuardAccount? Account { get; set; }
        public LoginType LoginType
        {
            get => _loginType;
            set => LoginTypeOnchange(ref value);
        }

        public string UserName
        {
            get => _userName;
            set => Set(ref _userName, value);
        }

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public string LoginExplanation
        {
            get => _loginExplanation;
            set => Set(ref _loginExplanation, value);
        }

        public bool UserNameEnabled
        {
            get => _userNameEnabled;
            set => Set(ref _userNameEnabled, value);
        }

        #endregion

        #region Commands

        public ICommand WindowOnLoadedCommand => new RelayCommand(o =>
        {
            if (o is not RoutedEventArgs { Source: Window window }) return;

            _thisWindow = window;
        });

        public ICommand LoginButtonCommand => new AsyncRelayCommand(async o =>
        {
            switch (LoginType)
            {
                case LoginType.Refresh:
                    await RefreshLogin();
                    return;
                case LoginType.Initial:
                    InitLogin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        #endregion

        #region PrivateMethods

        private void LoginTypeOnchange(ref LoginType loginType)
        {
            if (Account is null)
                throw new ArgumentNullException(nameof(Account));

            switch (loginType)
            {
                case LoginType.Initial:
                    break;
                case LoginType.Refresh:
                    UserName = Account.AccountName;
                    UserNameEnabled = false;
                    LoginExplanation = "Your Steam credentials have expired. For trade and market confirmations to work properly, please login again.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginType), loginType, null);
            }

            Set(ref _loginType, loginType, nameof(LoginType));
        }

        private async Task RefreshLogin()
        {
            if (Account is null)
                throw new ArgumentNullException(nameof(Account));

            Int64 steamTime = await TimeAligner.GetSteamTimeAsync();
            Account.FullyEnrolled = true;

            UserLogin userLogin = new(UserName, Password);
            LoginResult response = LoginResult.BadCredentials;

            while ((response = userLogin.DoLogin()) != LoginResult.LoginOkay)
            {
                switch (response)
                {
                    case LoginResult.NeedCaptcha:

                        CaptchaWindowView window = new CaptchaWindowView();
                        var dataContext = (window.DataContext as CaptchaWindowViewModel)!;
                        dataContext.CaptchaGid = userLogin.CaptchaGid; //-V3149

                        if (window.ShowDialog() == false)
                        {
                            _thisWindow?.Close();
                            return;
                        }
                        userLogin.CaptchaText = dataContext.CaptchaCode;
                        break;

                    case LoginResult.Need2Fa:
                        userLogin.TwoFactorCode = Account.GenerateSteamGuardCodeForTime(steamTime);
                        break;

                    case LoginResult.BadRsa:
                        MessageBox.Show("Error logging in: Steam returned \"BadRSA\".", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.BadCredentials:
                        MessageBox.Show("Error logging in: Username or password was incorrect.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.TooManyFailedLogins:
                        MessageBox.Show("Error logging in: Too many failed logins, try again later.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.GeneralFailure:
                        MessageBox.Show("Error logging in: Steam returned \"GeneralFailure\".", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Account.Session = userLogin.Session;
            await HandlingAccount(true);
            _thisWindow?.Close();
        }

        private void InitLogin()
        {

        }

        private async Task HandlingAccount(bool isRefreshing = false)
        {
            if (Account is null)
                throw new ArgumentNullException(nameof(Account));

            await ManifestModelService.SaveAccountInGoogleDrive(Account);

            if (isRefreshing)
            {
                MessageBox.Show("Your login session was refreshed.");
            }
            else
            {
                MessageBox.Show("Mobile authenticator successfully linked. Please write down your revocation code: " + Account.RevocationCode);
            }
        }

        #endregion

    }
}
