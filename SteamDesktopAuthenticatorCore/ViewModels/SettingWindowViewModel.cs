using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WpfHelper;
using WpfHelper.Commands;
using WpfHelper.Custom;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class SettingWindowViewModel : BaseViewModel
    {
        public SettingWindowViewModel()
        {
            var manifest = App.ManifestModelService.GetManifestModel();

            _periodicCheckingInterval = manifest.PeriodicCheckingInterval.ToString();
            _autoConfirmMarketTransactions = manifest.AutoConfirmMarketTransactions;
            _autoConfirmTrades = manifest.AutoConfirmTrades;
            _checkAllAccounts = manifest.CheckAllAccounts;
        }

        #region Variables

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
            set
            {
                if (!AutoConfirmTrades)
                    if (CustomMessageBox.Show("Be careful with this option. Turn it on?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        return;

                Set(ref _autoConfirmTrades, value);
            }
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

        public ICommand OnWindowCloseCommand => new RelayCommand(o =>
        {
            var manifest = App.ManifestModelService.GetManifestModel();

            manifest.PeriodicCheckingInterval = int.Parse(PeriodicCheckingInterval);
            manifest.AutoConfirmMarketTransactions = AutoConfirmMarketTransactions;
            manifest.AutoConfirmTrades = AutoConfirmTrades;
            manifest.CheckAllAccounts = CheckAllAccounts;

            App.ManifestModelService.SaveManifest();
        });

        #endregion

    }
}
