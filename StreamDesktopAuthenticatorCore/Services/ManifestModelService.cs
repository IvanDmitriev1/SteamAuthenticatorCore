using System;
using System.Threading.Tasks;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Models;

namespace SteamDesktopAuthenticatorCore.Services
{
    static partial class ManifestModelService
    {
        private const string ManifestFileName = "manifest.json";
        private static ManifestModel? _manifest;
        private static SettingsModel? _settings;

        public static async Task<ManifestModel> GetManifest()
        {
            if (_settings is null)
            {
                if (await SettingsModelService.GetSettingsModel() is not { } settings)
                    throw new ArgumentNullException(nameof(settings));

                _settings = settings;
            }

            return _settings.ManifestLocation switch
            {
                ManifestLocation.Drive => await GetManifestFromDrive(),
                ManifestLocation.GoogleDrive => await GetManifestFromGoogleDrive(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task SaveManifest()
        {
            if (_settings is null)
                throw new ArgumentNullException(nameof(_settings));

            switch (_settings.ManifestLocation)
            {
                case ManifestLocation.Drive:
                    await SaveManifestInDrive();
                    break;
                case ManifestLocation.GoogleDrive:
                    await SaveManifestInGoogleFile();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task GetAccounts()
        {
            if (_settings is null)
                throw new ArgumentNullException(nameof(_settings));

            switch (_settings.ManifestLocation)
            {
                case ManifestLocation.Drive:
                    await GetAccountsInDrive();
                    break;
                case ManifestLocation.GoogleDrive:
                    await GetAccountsInGoogleDrive();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task AddSteamGuardAccount(string fileName, string filePath)
        {
            if (_settings is null)
                throw new ArgumentNullException(nameof(_settings));

            switch (_settings.ManifestLocation)
            {
                case ManifestLocation.Drive:
                    await AddSteamGuardAccountInDrive(fileName, filePath);
                    break;
                case ManifestLocation.GoogleDrive:
                    await AddSteamGuardAccountInGoogleDrive(fileName, filePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task SaveSteamGuardAccount(SteamGuardAccount? account)
        {
            if (_settings is null)
                throw new ArgumentNullException(nameof(_settings));

            switch (_settings.ManifestLocation)
            {
                case ManifestLocation.Drive:
                    await SaveManifestInDrive();
                    break;
                case ManifestLocation.GoogleDrive:
                    if (account is null)
                        throw new ArgumentNullException(nameof(account));

                    await SaveAccountInGoogleDrive(account);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            if (_settings is null)
                throw new ArgumentNullException(nameof(_settings));

            switch (_settings.ManifestLocation)
            {
                case ManifestLocation.Drive:
                    await DeleteSteamGuardAccountInDrive(account);
                    break;
                case ManifestLocation.GoogleDrive:
                    await DeleteSteamGuardAccountInGoogleDrive(account);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}