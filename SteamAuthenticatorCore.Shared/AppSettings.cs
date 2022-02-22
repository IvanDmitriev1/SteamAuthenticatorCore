namespace SteamAuthenticatorCore.Shared;

public class AppSettings : BaseViewModel, ISettings
    {
        public AppSettings(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            IsInitialized = false;
        }

        public enum ManifestLocationModel
        {
            None,
            LocalDrive,
            GoogleDrive
        }

        private readonly ISettingsService _settingsService;

        private ManifestLocationModel _manifestLocation;
        private bool _firstRun;
        private bool _updated;
        private int _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;

        [IgnoreSettings]
        public bool IsInitialized { get; private set; }

        public ManifestLocationModel ManifestLocation
        {
            get => _manifestLocation;
            set => SetWithoutCheck(ref _manifestLocation, value);
        }

        public bool FirstRun
        {
            get => _firstRun;
            set => SetWithoutCheck(ref _firstRun, value);
        }

        public bool Updated
        {
            get => _updated;
            set => SetWithoutCheck(ref _updated, value);
        }

        public int PeriodicCheckingInterval
        {
            get => _periodicCheckingInterval;
            set
            {
                if (value < 10)
                    return;

                SetWithoutCheck(ref _periodicCheckingInterval, value);
            }
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set => SetWithoutCheck(ref _autoConfirmMarketTransactions, value);
        }


        public void DefaultSettings()
        {
            ManifestLocation = ManifestLocationModel.LocalDrive;
            FirstRun = true;
            Updated = false;
            PeriodicCheckingInterval = 10;
            AutoConfirmMarketTransactions = false;

            IsInitialized = true;
        }

        public void LoadSettings()
        {
            DefaultSettings();
            _settingsService.LoadSettings(this);
        }

        public void SaveSettings()
        {
            _settingsService.SaveSettings(this);
        }
    }