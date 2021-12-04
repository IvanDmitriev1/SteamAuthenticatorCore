using SteamAuthCore;
using SteamAuthenticatorAndroid.Services;

namespace SteamAuthenticatorAndroid.ViewModels
{
    class SettingsPageViewModel : BaseViewModel
    {
        public SettingsPageViewModel()
        {
            _manifest = ManifestModelService.GetManifest().Result;
            _periodicCheckingInterval = string.Empty;

            _periodicCheckingInterval = _manifest.PeriodicCheckingInterval.ToString();
            _autoConfirmMarketTransactions = _manifest.AutoConfirmMarketTransactions;
            _autoConfirmTrades = _manifest.AutoConfirmTrades;
        }

        #region Variables

        private ManifestModel _manifest;

        private string _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;
        private bool _autoConfirmTrades;

        #endregion

        #region Fields

        public static MainPageViewModel? Page { get; set; }

        public string PeriodicCheckingInterval
        {
            get => _periodicCheckingInterval;
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                int num = int.Parse(value);
                if (num < 1)
                    return;

                _manifest.PeriodicCheckingInterval = num;
                Page!.AutoConfirmTradesTimer.Interval = num;
                SetProperty(ref _periodicCheckingInterval, value);
            }
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set
            {
                _manifest.AutoConfirmMarketTransactions = value;

                if (value)
                {
                    Page!.AutoConfirmTradesTimer.Start();
                }
                else
                {
                    Page!.AutoConfirmTradesTimer.Stop();
                }

                SetProperty(ref _autoConfirmMarketTransactions, value);
            }
        }

        public bool AutoConfirmTrades
        {
            get => _autoConfirmTrades;
            set
            {
                _manifest.AutoConfirmTrades = value;
                SetProperty(ref _autoConfirmTrades, value);
            }
        }

        #endregion
    }
}
