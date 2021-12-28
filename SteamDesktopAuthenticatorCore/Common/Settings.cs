using WpfHelper.Common;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.Common
{
    public class AppSettings : BaseViewModel, ISettings
    {
        public AppSettings()
        {
            DefaultSettings();
        }

        public enum ManifestLocationModel
        {
            Drive,
            GoogleDrive
        }


        private ManifestLocationModel _manifestLocation;
        private bool _firstRun;
        private bool _updated;
        private int _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;
        private bool _checkAllAccounts;


        public ManifestLocationModel ManifestLocation
        {
            get => _manifestLocation;
            set => Set(ref _manifestLocation, value);
        }

        public bool FirstRun
        {
            get => _firstRun;
            set => Set(ref _firstRun, value);
        }

        public bool Updated
        {
            get => _updated;
            set => Set(ref _updated, value);
        }

        public int PeriodicCheckingInterval
        {
            get => _periodicCheckingInterval;
            set
            {
                if (value == 0)
                    return;

                Set(ref _periodicCheckingInterval, value);
            }
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set => Set(ref _autoConfirmMarketTransactions, value);
        }

        public bool CheckAllAccounts
        {
            get => _checkAllAccounts;
            set => Set(ref _checkAllAccounts, value);
        }


        public void DefaultSettings()
        {
            ManifestLocation = ManifestLocationModel.Drive;
            FirstRun = true;
            Updated = false;
            PeriodicCheckingInterval = 10;
            AutoConfirmMarketTransactions = false;
            CheckAllAccounts = true;
        }
    }
}
