using System;
using SteamDesktopAuthenticatorCore.Models;
using Microsoft.Win32;

namespace SteamDesktopAuthenticatorCore.Services
{
    static class SettingsModelService
    {
        private static SettingsModel? _settingsModel;

        public static SettingsModel GetSettingsModel()
        {
            if (_settingsModel is not null)
                return _settingsModel;

            using RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software")!;
            using RegistryKey? key = softwareKey.OpenSubKey($"{App.Name}", true);

            if (key is null)
            {
                _settingsModel = CreateSettings();
                SaveSettings();
                return _settingsModel;
            }

            if (key.GetValue("File location") is not string filesLocation)
            {
                key.SetValue("File location", $"{SettingsModel.ManifestLocationModel.Drive}");
                filesLocation = $"{SettingsModel.ManifestLocationModel.Drive}";
            }

            if (!bool.TryParse((string?)key.GetValue("FirstRun"), out var firstRun))
            {
                key.SetValue("FirstRun", true);
                firstRun = true;
            }

            _settingsModel = new SettingsModel()
            {
                ManifestLocation = filesLocation switch
                {
                    nameof(SettingsModel.ManifestLocationModel.Drive) => SettingsModel.ManifestLocationModel.Drive,
                    nameof(SettingsModel.ManifestLocationModel.GoogleDrive) => SettingsModel.ManifestLocationModel.GoogleDrive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                FirstRun = firstRun
            };

            return _settingsModel;
        }

        public static void SaveSettings()
        {
            if (_settingsModel is null)
                throw new ArgumentNullException();

            using RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
            using RegistryKey key = softwareKey.OpenSubKey($"{App.Name}", true) ?? softwareKey.CreateSubKey($"{App.Name}");

            key.SetValue("File location", $"{_settingsModel.ManifestLocation}");
            key.SetValue("FirstRun", $"{_settingsModel.FirstRun}");
        }

        private static SettingsModel CreateSettings()
        {
            SettingsModel settings = new SettingsModel()
            {
                ManifestLocation = SettingsModel.ManifestLocationModel.Drive,
                FirstRun = true
            };

            return settings;
        }
    }
}
