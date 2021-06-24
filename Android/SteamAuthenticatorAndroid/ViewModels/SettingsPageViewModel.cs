using System;
using System.Threading.Tasks;
using SteamAuthCore.Models;
using SteamAuthenticatorAndroid.Services;

namespace SteamAuthenticatorAndroid.ViewModels
{
    class SettingsPageViewModel : BaseViewModel
    {
        public SettingsPageViewModel()
        {
            _manifest = new ManifestModel();
            _periodicCheckingInterval = string.Empty;

            Task.Run( async () =>
            {
                _manifest = await ManifestModelService.GetManifest();

                _periodicCheckingInterval = _manifest.PeriodicCheckingInterval.ToString();
                _autoConfirmMarketTransactions = _manifest.AutoConfirmMarketTransactions;
                _autoConfirmTrades = _manifest.AutoConfirmTrades;
            });

            
        }

        #region Variables

        private ManifestModel _manifest;

        private string _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;
        private bool _autoConfirmTrades;

        #endregion

        #region Fields
        public string PeriodicCheckingInterval
        {
            get => _periodicCheckingInterval;
            set => SetProperty(ref _periodicCheckingInterval, value);
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set => SetProperty(ref _autoConfirmMarketTransactions, value);
        }

        public bool AutoConfirmTrades
        {
            get => _autoConfirmTrades;
            set => SetProperty(ref _autoConfirmTrades, value);
        }

        #endregion
    }
}
