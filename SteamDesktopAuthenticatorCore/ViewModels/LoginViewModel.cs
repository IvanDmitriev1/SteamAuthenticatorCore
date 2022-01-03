using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using WpfHelper.Commands;
using BaseViewModel = WPFUI.Common.BaseViewModel;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            
        }

        private SteamGuardAccount? _account;
        private string _password = string.Empty;
        private string _userName= string.Empty;


        public SteamGuardAccount? Account
        {
            get => _account;
            set
            {
                Set(ref _account, value);
                OnPropertyChanged(nameof(IsEnabledUserNameTextBox));
            }
        }

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public string UserName
        {
            get => Account is not null ? Account.AccountName : _userName;
            set => Set(ref _userName, value);
        }

        public bool IsEnabledUserNameTextBox => Account is null;


        public ICommand LoginCommand => new AsyncRelayCommand(async o =>
        {
            if (Account is null)
            {
                await InitLogin();
                return;
            }

            await RefreshLogin();
        });

        private async Task RefreshLogin()
        {
            UserLogin userLogin = new(UserName, Password);
            Int64 steamTime = await TimeAligner.GetSteamTimeAsync();

            switch (userLogin.DoLogin())
            {
                case LoginResult.LoginOkay:
                    break;
                case LoginResult.GeneralFailure:
                    break;
                case LoginResult.BadRsa:
                    break;
                case LoginResult.BadCredentials:
                    break;
                case LoginResult.NeedCaptcha:
                    break;
                case LoginResult.Need2Fa:
                    break;
                case LoginResult.NeedEmail:
                    break;
                case LoginResult.TooManyFailedLogins:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private async Task InitLogin()
        {

        }
    }
}
