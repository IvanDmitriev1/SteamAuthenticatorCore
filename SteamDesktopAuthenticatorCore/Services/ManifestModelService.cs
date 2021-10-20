﻿using System;
using System.Threading.Tasks;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Models;

namespace SteamDesktopAuthenticatorCore.Services
{
    static partial class ManifestModelService
    {
        private static ManifestModel? _manifest;
        private static SettingsModel? _settings;

        public static void SetManifest(ref ManifestModel manifest)
        {
            _manifest = new ManifestModel(manifest);
        }

        public static async Task<ManifestModel> GetManifest()
        {
            CheckSettings();

            return _settings!.ManifestLocation switch
            {
                SettingsModel.ManifestLocationModel.Drive => await GetManifestFromDrive(),
                SettingsModel.ManifestLocationModel.GoogleDrive => await GetManifestFromGoogleDrive(),
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

        private static void CheckSettings()
        {
            if (_settings is null)
            {
                if (SettingsModelService.GetSettingsModel() is not { } settings)
                    throw new ArgumentNullException(nameof(settings));

                _settings = settings;
            }
        }

        private static async Task CheckSettings(Func<Task> onDriveMethod, Func<Task> onGoogleDriveAction)
        {
            CheckSettings();

            switch (_settings!.ManifestLocation)
            {
                case SettingsModel.ManifestLocationModel.Drive:
                    await onDriveMethod.Invoke();
                    break;
                case SettingsModel.ManifestLocationModel.GoogleDrive:
                    await onGoogleDriveAction.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task CheckSettings(Task onDriveMethod, Task onGoogleDriveAction)
        {
            CheckSettings();

            switch (_settings!.ManifestLocation)
            {
                case SettingsModel.ManifestLocationModel.Drive:
                    await onDriveMethod;
                    break;
                case SettingsModel.ManifestLocationModel.GoogleDrive:
                    await onGoogleDriveAction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}