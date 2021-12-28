using System;
using System.Collections.Generic;
using WpfHelper.Common;

namespace SteamDesktopAuthenticatorCore.Common
{
    internal class Settings : BaseSettings
    {
        public Settings() : base(App.Name)
        {
            SettingsMap = new Dictionary<Type, ISettings>()
            {
                {typeof(AppSettings), new AppSettings()}
            };
        }

        private static Settings? _settingsModel;

        public static Settings GetSettings()
        {
            if (_settingsModel is not null)
                return _settingsModel;

            _settingsModel = new Settings();
            CreateBaseSettings(_settingsModel, App.Name);

            return _settingsModel;
        }
    }

    internal class AppSettings : BaseViewModel, ISettings
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


        public void DefaultSettings()
        {
            ManifestLocation = ManifestLocationModel.Drive;
            FirstRun = true;
            Updated = false;
        }
    }
}
