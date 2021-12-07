using System.Windows.Input;
using SteamAuthCore.Manifest;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            _manifestModelService = App.ManifestModelService;

            var manifest = _manifestModelService.GetManifestModel();
            _tradePeriodicCheckingInterval = manifest.PeriodicCheckingInterval;
            _autoConfirmMarket = manifest.AutoConfirmMarketTransactions;
        }

        private readonly IManifestModelService _manifestModelService;

        private int _tradePeriodicCheckingInterval;
        private bool _autoConfirmMarket;

        public int TradePeriodicCheckingInterval
        {
            get => _tradePeriodicCheckingInterval;
            set => SetProperty(ref _tradePeriodicCheckingInterval, value);
        }

        public bool AutoConfirmMarket
        {
            get => _autoConfirmMarket;
            set => SetProperty(ref _autoConfirmMarket, value);
        }

        public ICommand OnClosingCommand => new Command(() =>
        {
            var manifest = _manifestModelService.GetManifestModel();
            manifest.AutoConfirmMarketTransactions = AutoConfirmMarket;
            manifest.PeriodicCheckingInterval = TradePeriodicCheckingInterval;

            _manifestModelService.SaveManifest();
        });
    }
}
