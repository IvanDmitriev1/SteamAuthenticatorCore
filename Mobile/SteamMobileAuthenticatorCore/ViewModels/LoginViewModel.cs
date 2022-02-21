using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.ViewModels
{
    public class LoginViewModel : BaseViewModel, IQueryAttributable
    {
        public LoginViewModel()
        {
            _accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
            _manifestModelService = DependencyService.Get<IManifestModelService>();
            _selectedAccount = new SteamGuardAccount();
            _password = string.Empty;
        }

        #region Properties

        private readonly ObservableCollection<SteamGuardAccount> _accounts;
        private readonly IManifestModelService _manifestModelService;
        private SteamGuardAccount _selectedAccount;
        private string _password;

        public SteamGuardAccount SelectedAccount
        {
            get => _selectedAccount;
            private set => SetProperty(ref _selectedAccount, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        #endregion

        public void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            var id= HttpUtility.UrlDecode(query["id"]);

            SelectedAccount = _accounts[Convert.ToInt32(id)];
        }

        public ICommand OnLoginCommand => new AsyncCommand(async () =>
        {
            if (string.IsNullOrEmpty(SelectedAccount.SharedSecret))
                return;

            Int64 steamTime = await TimeAligner.GetSteamTimeAsync();
            SelectedAccount.FullyEnrolled = true;

            UserLogin userLogin = new(SelectedAccount.AccountName, Password);
            LoginResult response;

            switch (response = userLogin.DoLogin())
            {
                case LoginResult.NeedCaptcha:
                    await Application.Current.MainPage.DisplayAlert("Need captcha", "Not done yet", "Ok");
                    break;

                case LoginResult.Need2Fa:
                    userLogin.TwoFactorCode = SelectedAccount.GenerateSteamGuardCodeForTime(steamTime);
                    response = userLogin.DoLogin();
                    break;

                case LoginResult.BadRsa:
                    await Application.Current.MainPage.DisplayAlert("Login error", "Error logging in: Steam returned \"BadRSA\".", "Ok");
                    break;

                case LoginResult.BadCredentials:
                    await Application.Current.MainPage.DisplayAlert("Error", "Error logging in: invalid username or password.", "Ok");
                    break;

                case LoginResult.TooManyFailedLogins:
                    await Application.Current.MainPage.DisplayAlert("Login error", "Error logging in: Too many failed logins, try again later.", "Ok");
                    break;
                case LoginResult.GeneralFailure:
                    await Application.Current.MainPage.DisplayAlert("Login error", "Error logging in: Steam returned \"GeneralFailure\".", "Ok");
                    break;
                case LoginResult.LoginOkay:
                case LoginResult.NeedEmail:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (response == LoginResult.LoginOkay)
            {
                SelectedAccount.Session = userLogin.Session;
                await _manifestModelService.SaveSteamGuardAccount(SelectedAccount);
                await Application.Current.MainPage.DisplayAlert("Login", "The session has been refreshed", "Ok");
            }

            await Shell.Current.GoToAsync("..");
        });
    }
}
