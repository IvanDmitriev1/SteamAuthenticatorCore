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
            set => Set(ref _periodicCheckingInterval, value);
        }


        public void DefaultSettings()
        {
            ManifestLocation = ManifestLocationModel.Drive;
            FirstRun = true;
            Updated = false;
        }
    }
}
