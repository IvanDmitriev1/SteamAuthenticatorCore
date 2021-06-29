using System.Text.RegularExpressions;
using System.Windows.Input;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Services;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class SettingWindowViewModel : BaseViewModel
    {
        public SettingWindowViewModel()
        {
            if (!App.InDesignMode)
            {
                _manifest = ManifestModelService.GetManifest().Result;
            }
            else
            {
                _manifest = new ManifestModel();
            }
            
            _periodicCheckingInterval = _manifest.PeriodicCheckingInterval.ToString();
            _autoConfirmMarketTransactions = _manifest.AutoConfirmMarketTransactions;
            _autoConfirmTrades = _manifest.AutoConfirmTrades;
            _checkAllAccounts = _manifest.CheckAllAccounts;
        }

        #region Variables

        private readonly ManifestModel _manifest;
        private static readonly Regex Regex = new("[^0-9]+");

        private string _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;
        private bool _autoConfirmTrades;
        private bool _checkAllAccounts;

        #endregion

        #region Fields
        public string PeriodicCheckingInterval
        {
            get => _periodicCheckingInterval;
            set => Set(ref _periodicCheckingInterval, value);
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set => Set(ref _autoConfirmMarketTransactions, value);
        }

        public bool AutoConfirmTrades
        {
            get => _autoConfirmTrades;
            set => Set(ref _autoConfirmTrades, value);
        }

        public bool CheckAllAccounts
        {
            get => _checkAllAccounts;
            set => Set(ref _checkAllAccounts, value);
        }

        #endregion

        #region Commands

        public ICommand OnTextBoxPreviewInputCommand => new RelayCommand(o =>
        {
            if (o is not TextCompositionEventArgs args) return;

            args.Handled = Regex.IsMatch(args.Text);
        });

        public ICommand OnWindowCloseCommand => new AsyncRelayCommand(async o =>
        {
            _manifest.PeriodicCheckingInterval = int.Parse(PeriodicCheckingInterval);
            _manifest.AutoConfirmMarketTransactions = AutoConfirmMarketTransactions;
            _manifest.AutoConfirmTrades = AutoConfirmTrades;
            _manifest.CheckAllAccounts = CheckAllAccounts;

            await ManifestModelService.SaveManifest();
        });

        #endregion

    }
}
