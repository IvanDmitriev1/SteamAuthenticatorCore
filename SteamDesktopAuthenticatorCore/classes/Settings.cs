using System;
using Microsoft.Win32;

namespace SteamDesktopAuthenticatorCore.classes
{
    class Settings
    {
        #region Fields
        public enum ManifestLocationModel
        {
            Drive,
            GoogleDrive
        }
        public ManifestLocationModel ManifestLocation { get; set; }
        public bool FirstRun { get; set; }
        public bool Updated { get; set; }

        public bool ImportFiles { get; set; } = false;

        #endregion

        private static Settings? _settings;

        #region PublicMethods
        public static Settings GetSettings()
        {
            if (_settings is not null)
                return _settings;

            using RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software")!;
            using RegistryKey? key = softwareKey.OpenSubKey($"{App.Name}", true);

            if (key is null)
            {
                _settings = CreateSettings();
                SaveSettingsPr();
                return _settings;
            }

            _settings = CreateSettings(key);
            return _settings;
        }

        public void SaveSettings()
        {
            SaveSettingsPr();
        }

        #endregion

        #region PrivateMethods

        private static void SaveSettingsPr()
        {
            if (_settings is null)
                throw new ArgumentNullException();

            using RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
            using RegistryKey key = softwareKey.OpenSubKey($"{App.Name}", true) ?? softwareKey.CreateSubKey($"{App.Name}");

            key.SetValue($"{nameof(_settings.ManifestLocation)}", $"{_settings.ManifestLocation}");
            key.SetValue($"{nameof(_settings.FirstRun)}", $"{_settings.FirstRun}");
            key.SetValue($"{nameof(_settings.Updated)}", $"{_settings.Updated}");
        }

        private static Settings CreateSettings()
        {
            Settings settings = new()
            {
                ManifestLocation = ManifestLocationModel.Drive,
                FirstRun = true,
                Updated = false
            };

            return settings;
        }

        private static Settings CreateSettings(RegistryKey key)
        {
            if (key.GetValue($"{nameof(_settings.ManifestLocation)}") is not string filesLocation)
            {
                key.SetValue($"{nameof(_settings.ManifestLocation)}", $"{ManifestLocationModel.Drive}");
                filesLocation = $"{ManifestLocationModel.Drive}";
            }

            if (!bool.TryParse((string?)key.GetValue($"{nameof(_settings.FirstRun)}"), out var firstRun))
            {
                key.SetValue($"{nameof(_settings.FirstRun)}", true);
                firstRun = true;
            }

            if (!bool.TryParse((string?)key.GetValue($"{nameof(_settings.Updated)}"), out var updated))
            {
                key.SetValue($"{nameof(_settings.Updated)}", false);
                updated = false;
            }

            return new Settings()
            {
                ManifestLocation = filesLocation switch
                {
                    nameof(ManifestLocationModel.Drive) => ManifestLocationModel.Drive,
                    nameof(ManifestLocationModel.GoogleDrive) => ManifestLocationModel.GoogleDrive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                FirstRun = firstRun,
                Updated = updated
            };
        }

        #endregion
    }
}
