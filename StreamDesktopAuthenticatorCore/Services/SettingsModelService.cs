using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamDesktopAuthenticatorCore.Models;

namespace SteamDesktopAuthenticatorCore.Services
{
    static class SettingsModelService
    {
        private static SettingsModel? _settingsModel;
        private static readonly string SettingsPath = Path.Combine(Path.GetTempPath(), "SDA_settings.json");

        public static async Task<SettingsModel?> GetSettingsModel()
        {
            if (_settingsModel is not null)
                return _settingsModel;

            if (!File.Exists(SettingsPath))
            {
                await CreateSettings();
                return null;
            }

            if (JsonConvert.DeserializeObject<SettingsModel>(await File.ReadAllTextAsync(SettingsPath)) is not { } settings)
                throw new ArgumentNullException(nameof(settings));

            _settingsModel = settings;
            return _settingsModel;
        }

        public static async Task SaveSettings()
        {
            if (_settingsModel is null)
                throw new ArgumentNullException();

            string serialized = JsonConvert.SerializeObject(_settingsModel);
            await File.WriteAllTextAsync(SettingsPath, serialized);
        }

        private static async Task CreateSettings()
        {
            _settingsModel = new SettingsModel()
            {
                ManifestLocation = ManifestLocation.Drive
            };

            await SaveSettings();
        }
    }
}
