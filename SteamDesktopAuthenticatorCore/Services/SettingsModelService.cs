using System;
using SteamDesktopAuthenticatorCore.Models;
using Microsoft.Win32;

namespace SteamDesktopAuthenticatorCore.Services
{
    static class SettingsModelService
    {
        private static SettingsModel? _settingsModel;

        public static SettingsModel? GetSettingsModel()
        {
            if (_settingsModel is not null)
                return _settingsModel;

            using RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software")!;
            using RegistryKey? key = softwareKey.OpenSubKey($"{App.Name}");

            if (key is null)
            {
                CreateSettings();
                return null;
            }

            if (key.GetValue("File location") is not string value)
                throw new ArgumentException();

            _settingsModel = new SettingsModel()
            {
                ManifestLocation = value switch
                {
                    nameof(SettingsModel.ManifestLocationModel.Drive) => SettingsModel.ManifestLocationModel.Drive,
                    nameof(SettingsModel.ManifestLocationModel.GoogleDrive) => SettingsModel.ManifestLocationModel.GoogleDrive,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };

            key.Close();
            return _settingsModel;
        }

        public static void SaveSettings()
        {
            if (_settingsModel is null)
                throw new ArgumentNullException();

            using RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
            using RegistryKey key = softwareKey.OpenSubKey($"{App.Name}", true) ?? softwareKey.CreateSubKey($"{App.Name}");

            key.SetValue("File location", $"{_settingsModel.ManifestLocation}");
        }

        private static void CreateSettings()
        {
            _settingsModel = new SettingsModel()
            {
                ManifestLocation = SettingsModel.ManifestLocationModel.Drive
            };

            SaveSettings();
        }
    }
}
