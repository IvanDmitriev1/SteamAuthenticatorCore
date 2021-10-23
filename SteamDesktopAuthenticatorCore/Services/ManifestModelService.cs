using System;
using System.Threading.Tasks;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.classes;

namespace SteamDesktopAuthenticatorCore.Services
{
    static partial class ManifestModelService
    {
        private static ManifestModel? _manifest;

        public static void SetManifest(ManifestModel manifest)
        {
            _manifest = new ManifestModel(manifest);
        }

        public static async Task<ManifestModel> GetManifest()
        {
            var settings = Settings.GetSettings();

            return settings.ManifestLocation switch
            {
                Settings.ManifestLocationModel.Drive => await GetManifestFromDrive(),
                Settings.ManifestLocationModel.GoogleDrive => await GetManifestFromGoogleDrive(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task SaveManifest()
        {
            await CheckSettings(SaveManifestInDrive, SaveManifestInGoogleFile);
        }

        public static async Task GetAccounts()
        {
            await CheckSettings(GetAccountsInDrive, GetAccountsInGoogleDrive);
        }

        public static async Task AddSteamGuardAccount(string fileName, string fileData)
        {
            await CheckSettings(AddSteamGuardAccountInDrive(fileName, fileData), AddSteamGuardAccountInGoogleDrive(fileName, fileData));
        }

        public static async Task SaveSteamGuardAccount(SteamGuardAccount account)
        {
            await CheckSettings(SaveSteamGuardAccountInDrive(account), SaveAccountInGoogleDrive(account));
        }

        public static async Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            await CheckSettings(DeleteSteamGuardAccountInDrive(account), DeleteSteamGuardAccountInGoogleDrive(account));
        }

        #region PrivateMethods

        private static async Task CheckSettings(Func<Task> onDriveMethod, Func<Task> onGoogleDriveAction)
        {
            var settings = Settings.GetSettings();

            switch (settings.ManifestLocation)
            {
                case Settings.ManifestLocationModel.Drive:
                    await onDriveMethod.Invoke();
                    break;
                case Settings.ManifestLocationModel.GoogleDrive:
                    await onGoogleDriveAction.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task CheckSettings(Task onDriveMethod, Task onGoogleDriveAction)
        {
            var settings = Settings.GetSettings();

            switch (settings.ManifestLocation)
            {
                case Settings.ManifestLocationModel.Drive:
                    await onDriveMethod;
                    break;
                case Settings.ManifestLocationModel.GoogleDrive:
                    await onGoogleDriveAction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}