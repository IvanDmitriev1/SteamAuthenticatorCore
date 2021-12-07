using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;
using WpfHelper.Custom;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class LoginWindowViewModel : BaseViewModel
    {
        public LoginWindowViewModel()
        {
            _manifestModelService = App.ManifestModelService;

            LoginType = LoginType.Initial;
            _loginExplanation = "This will activate Steam Desktop Authenticator on your Steam account. This requires a phone number that can receive SMS.";
        }

        #region Variables

        private readonly IManifestModelService _manifestModelService;
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
            set
            {
                if (Account is null)
                {
                    Set(ref _loginType, value);
                    return;
                }
                
                LoginTypeOnchange(ref value);
            }
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
                    await InitLogin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        #endregion

        private async Task RefreshLogin()
        {
            if (Account is null)
                throw new ArgumentNullException(nameof(Account));

            Account.FullyEnrolled = true;
            UserLogin userLogin = new(UserName, Password);

            LoginResult response = LoginResult.BadCredentials;
            Int64 steamTime = await TimeAligner.GetSteamTimeAsync();
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
                        CustomMessageBox.Show("Error logging in: Steam returned \"BadRSA\".", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.BadCredentials:
                        CustomMessageBox.Show("Error logging in: Username or password was incorrect.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.TooManyFailedLogins:
                        CustomMessageBox.Show("Error logging in: Too many failed logins, try again later.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.GeneralFailure:
                        CustomMessageBox.Show("Error logging in: Steam returned \"GeneralFailure\".", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task InitLogin()
        {
            UserLogin userLogin = new(UserName, Password);
            LoginResult response = LoginResult.BadCredentials;

            while ((response = userLogin.DoLogin()) != LoginResult.LoginOkay)
            {
                switch (response)
                {
                    case LoginResult.NeedEmail:

                        InputWindowView inputWindow = new();
                        var inputWindowDataContext = (inputWindow.DataContext as InputWindowViewModel)!;
                        inputWindowDataContext.Text = "Enter your phone number in the following format: +{cC} phoneNumber. EG, +1 123-456-7890";
                        inputWindowDataContext.InputString = "+1";

                        if (inputWindow.ShowDialog() == false)
                        {
                            _thisWindow?.Close();
                            return;
                        }
                        userLogin.EmailCode = inputWindowDataContext.InputString;
                        break;
                    case LoginResult.NeedCaptcha:

                        CaptchaWindowView captchaWindow = new();
                        var captchaWindowDataContext = (captchaWindow.DataContext as CaptchaWindowViewModel)!;
                        captchaWindowDataContext.CaptchaGid = userLogin.CaptchaGid; //-V3149

                        if (captchaWindow.ShowDialog() == false)
                        {
                            _thisWindow?.Close();
                            return;
                        }
                        userLogin.CaptchaText = captchaWindowDataContext.CaptchaCode;
                        break;
                    case LoginResult.Need2Fa:
                        CustomMessageBox.Show("This account already has a mobile authenticator linked to it.\nRemove the old authenticator from your Steam account before adding a new one.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.BadRsa:
                        CustomMessageBox.Show("Error logging in: Steam returned \"BadRSA\".", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.BadCredentials:
                        CustomMessageBox.Show("Error logging in: Username or password was incorrect.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.TooManyFailedLogins:
                        CustomMessageBox.Show("Error logging in: Too many failed logins, try again later.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;

                    case LoginResult.GeneralFailure:
                        CustomMessageBox.Show("Error logging in: Steam returned \"GeneralFailure\".", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _thisWindow?.Close();
                        return;
                    case LoginResult.LoginOkay:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //Login succeeded
            SessionData session = userLogin.Session;
            AuthenticatorLinker linker = new(session);

            AuthenticatorLinker.LinkResult linkResponse = AuthenticatorLinker.LinkResult.GeneralFailure;
            while ((linkResponse = linker.AddAuthenticator()) != AuthenticatorLinker.LinkResult.AwaitingFinalization)
            {
                switch (linkResponse)
                {
                    case AuthenticatorLinker.LinkResult.MustProvidePhoneNumber:
                        string phoneNumber = "";
                        while (!PhoneNumberOkay(phoneNumber))
                        {
                            InputWindowView phoneNumberWindow = new();
                            var inputWindowDataContext = (phoneNumberWindow.DataContext as InputWindowViewModel)!;
                            inputWindowDataContext.Text = "Enter your phone number in the following format: +{cC} phoneNumber. EG, +1 123-456-7890";
                            inputWindowDataContext.InputString = "+1";

                            if (phoneNumberWindow.ShowDialog() == false)
                            {
                                _thisWindow?.Close();
                                return;
                            }
                            phoneNumber = FilterPhoneNumber(inputWindowDataContext.InputString);
                            break;
                        }
                        linker.PhoneNumber = phoneNumber;
                        break;

                    case AuthenticatorLinker.LinkResult.MustRemovePhoneNumber:
                        linker.PhoneNumber = null;
                        break;

                    case AuthenticatorLinker.LinkResult.MustConfirmEmail:
                        MessageBox.Show("Please check your email, and click the link Steam sent you before continuing.");
                        break;

                    case AuthenticatorLinker.LinkResult.GeneralFailure:
                        MessageBox.Show("Error adding your phone number. Steam returned \"GeneralFailure\".");
                        _thisWindow?.Close();
                        return;
                }
            }

            //Save the file immediately; losing this would be bad.
            await _manifestModelService.SaveSteamGuardAccount(linker.LinkedAccount);

            MessageBox.Show("The Mobile Authenticator has not yet been linked. Before finalizing the authenticator, please write down your revocation code: " + linker.LinkedAccount.RevocationCode);

            AuthenticatorLinker.FinalizeResult finalizeResponse = AuthenticatorLinker.FinalizeResult.GeneralFailure;
            while (finalizeResponse != AuthenticatorLinker.FinalizeResult.Success)
            {
                InputWindowView smsCodeWindow = new();
                var smsCodeDataContext = (smsCodeWindow.DataContext as InputWindowViewModel)!;
                smsCodeDataContext.Text = "Please input the SMS code sent to your phone.";

                if (smsCodeWindow.ShowDialog() == false)
                {
                    await _manifestModelService.DeleteSteamGuardAccount(linker.LinkedAccount);
                    _thisWindow?.Close();
                    return;
                }

                InputWindowView confirmRevocationCodeWindow = new();
                var confirmRevocationCodeDataContext = (smsCodeWindow.DataContext as InputWindowViewModel)!;
                smsCodeDataContext.Text = "Please enter your revocation code to ensure you've saved it.";
                confirmRevocationCodeWindow.ShowDialog();

                if (confirmRevocationCodeDataContext.InputString.ToUpper() != linker.LinkedAccount.RevocationCode)
                {
                    MessageBox.Show("Revocation code incorrect; the authenticator has not been linked.");
                    await _manifestModelService.DeleteSteamGuardAccount(linker.LinkedAccount);
                    _thisWindow?.Close();
                    return;
                }

                string smsCode = smsCodeDataContext.InputString;
                finalizeResponse = linker.FinalizeAddAuthenticator(smsCode);

                switch (finalizeResponse)
                {
                    case AuthenticatorLinker.FinalizeResult.BadSmsCode:
                        continue;

                    case AuthenticatorLinker.FinalizeResult.UnableToGenerateCorrectCodes:
                        MessageBox.Show("Unable to generate the proper codes to finalize this authenticator. The authenticator should not have been linked. In the off-chance it was, please write down your revocation code, as this is the last chance to see it: " + linker.LinkedAccount.RevocationCode);
                        await _manifestModelService.DeleteSteamGuardAccount(linker.LinkedAccount);
                        _thisWindow?.Close();
                        return;

                    case AuthenticatorLinker.FinalizeResult.GeneralFailure:
                        MessageBox.Show("Unable to finalize this authenticator. The authenticator should not have been linked. In the off-chance it was, please write down your revocation code, as this is the last chance to see it: " + linker.LinkedAccount.RevocationCode);
                        await _manifestModelService.DeleteSteamGuardAccount(linker.LinkedAccount);
                        _thisWindow?.Close();
                        return;
                    case AuthenticatorLinker.FinalizeResult.Success:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //Linked, finally. Re-save with FullyEnrolled property.
                await _manifestModelService.SaveSteamGuardAccount(linker.LinkedAccount);
                MessageBox.Show("Mobile authenticator successfully linked. Please write down your revocation code: " + linker.LinkedAccount.RevocationCode);
                _thisWindow?.Close();
            }

        }

        #region PrivateMethods

        private void LoginTypeOnchange(ref LoginType loginType)
        {
            switch (loginType)
            {
                case LoginType.Initial:
                    break;
                case LoginType.Refresh:
                    UserName = Account!.AccountName;
                    UserNameEnabled = false;
                    LoginExplanation = "Your Steam credentials have expired. For trade and market confirmations to work properly, please login again.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginType), loginType, null);
            }

            Set(ref _loginType, loginType, nameof(LoginType));
        }

        public string FilterPhoneNumber(in string phoneNumber)
        {
            return phoneNumber.Replace("-", "").Replace("(", "").Replace(")", "");
        }

        public bool PhoneNumberOkay(in string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return false;

            return phoneNumber[0] == '+';
        }

        private async Task HandlingAccount(bool isRefreshing = false)
        {
            if (Account is null)
                throw new ArgumentNullException(nameof(Account));

            await _manifestModelService.SaveSteamGuardAccount(Account);

            if (isRefreshing)
            {
                CustomMessageBox.Show("Your login session was refreshed.");
            }
            else
            {
                CustomMessageBox.Show("Mobile authenticator successfully linked. Please write down your revocation code: " + Account.RevocationCode);
            }
        }

        #endregion

    }
}