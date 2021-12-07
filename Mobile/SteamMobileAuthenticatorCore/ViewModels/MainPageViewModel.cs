using System.Collections.ObjectModel;
using System.Windows.Input;
using SteamAuthCore;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.ViewModels
{
    class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            Accounts = new ObservableCollection<SteamGuardAccount>();
        }

        #region Properties

        private SteamGuardAccount _selectedSteamGuardAccount;
        private double _progressBar;
        private string _loginToken;


        public ObservableCollection<SteamGuardAccount> Accounts { get; }

        public SteamGuardAccount SelectedSteamGuardAccount
        {
            get => _selectedSteamGuardAccount;
            set => SetProperty(ref _selectedSteamGuardAccount, value);
        }

        public double ProgressBar
        {
            get => _progressBar;
            set => SetProperty(ref _progressBar, value);
        }

        public string LoginToken
        {
            get => _loginToken;
            set => SetProperty(ref _loginToken, value);
        }

        #endregion

        public ICommand OnLoadCommand => new AsyncCommand(async () =>
        {
            Accounts.Clear();

            foreach (var accounts in await App.ManifestModelService.GetAccounts())
                Accounts.Add(accounts);
        });

        public ICommand ImportCommand => new Command(() =>
        {
            
        });

        public ICommand CopyCommand => new Command(() =>
        {

        });

    }
}
