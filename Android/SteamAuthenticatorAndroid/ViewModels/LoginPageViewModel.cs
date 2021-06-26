using System;
using SteamAuthCore;
using SteamAuthCore.Models;
using SteamAuthenticatorAndroid.Services;
using Xamarin.Forms;

namespace SteamAuthenticatorAndroid.ViewModels
{
    class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel()
        {
            if (Account is not null)
            {
                Login = Account.AccountName;
                LoginReadOnly = true;
            }

            LoginCommand = new Command(RefreshLogin);
        }

        #region Variabels

        private bool _loginReadOnly;
        private string _login = string.Empty;
        private string _password = string.Empty;

        #endregion

        #region Fields

        public static SteamGuardAccount? Account { get; set; }

        public bool LoginReadOnly
        {
            get => _loginReadOnly;
            set => SetProperty(ref _loginReadOnly, value);
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        #endregion

        public Command LoginCommand { get; }

        private async void RefreshLogin()
        {
            if (Account is null)
                throw new ArgumentNullException(nameof(Account));

            Int64 steamTime = await TimeAligner.GetSteamTimeAsync();
            Account.FullyEnrolled = true;

            UserLogin userLogin = new(Login, Password);
            LoginResult response = LoginResult.BadCredentials;

            while ((response = userLogin.DoLogin()) != LoginResult.LoginOkay)
            {
                switch (response)
                {
                    case LoginResult.NeedCaptcha:

                        /*CaptchaWindowView window = new CaptchaWindowView();
                        var dataContext = (window.DataContext as CaptchaWindowViewModel)!;
                        dataContext.CaptchaGid = userLogin.CaptchaGid; //-V3149*/

                        /*if (window.ShowDialog() == false)
                        {
                            _thisWindow?.Close();
                            return;
                        }
                        userLogin.CaptchaText = dataContext.CaptchaCode;*/
                        await Application.Current.MainPage.DisplayAlert("Not done yet", "Not done yet", "Ok");
                        break;

                    case LoginResult.Need2Fa:
                        userLogin.TwoFactorCode = Account.GenerateSteamGuardCodeForTime(steamTime);
                        break;

                    case LoginResult.BadRsa:
                        await Application.Current.MainPage.DisplayAlert("Login error" ,"Error logging in: Steam returned \"BadRSA\".", "Ok");
                        await Shell.Current.GoToAsync("..");
                        return;

                    case LoginResult.BadCredentials:
                        await Application.Current.MainPage.DisplayAlert("Error" ,"Error logging in: Username or password was incorrect.", "Ok");
                        await Shell.Current.GoToAsync("..");
                        return;

                    case LoginResult.TooManyFailedLogins:
                        await Application.Current.MainPage.DisplayAlert("Login error" ,"Error logging in: Too many failed logins, try again later.", "Ok");
                        await Shell.Current.GoToAsync("..");
                        return;

                    case LoginResult.GeneralFailure:
                        await Application.Current.MainPage.DisplayAlert("Login error" ,"Error logging in: Steam returned \"GeneralFailure\".", "Ok");
                        await Shell.Current.GoToAsync("..");
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Account.Session = userLogin.Session;

            await ManifestModelService.SaveManifest();
            await Application.Current.MainPage.DisplayAlert("Login", "Session was refreshed", "Ok");

            await Shell.Current.GoToAsync("..");
        }
    }
}
