using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GoogleDrive;
using Newtonsoft.Json;
using SteamAuthCore.Models;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace SteamDesktopAuthenticatorCore.Services
{
    public static partial class ManifestModelService
    {
        private static ManifestModel? _manifest;
        private static string _manifestFileName = "manifest.json";
        private static GoogleDriveApi? _api;

        public static async Task<ManifestModel> GetManifestFromGoogleDrive(GoogleDriveApi? api = null)
        {
            if (_manifest is not null)
                return _manifest;

            _api ??= api;

            if (_api is null)
                throw new ArgumentNullException(nameof(_api));

            if (await _api.CheckForFile(_manifestFileName) is not { } manifestFile)
            {
                await CreateNewManifestInGoogleDrive();
                return _manifest!;
            }

            GoogleDriveFileDownloader downloader = new(_api);
            downloader.OnDataDownloaded += (sender, args) =>
            {
                if (JsonConvert.DeserializeObject<ManifestModel>((string)args.UserState! ?? throw new InvalidOperationException()) is not { } manifest)
                    throw new ArgumentNullException(nameof(manifest));

                _manifest = manifest;
            };
            await downloader.Download(manifestFile);

            while (_manifest is null)
            {
                await Task.Delay(10);
            }

            await GetAccountsFromGoogleDrive();
            return _manifest;
        }

        public static async Task SaveManifestInGoogleFile()
        {
            if (_api is null || _manifest is null)
                throw new ArgumentNullException();

            ManifestModel newModel = new(_manifest);

            string serialized = JsonConvert.SerializeObject(newModel);
            await using MemoryStream stream = new(Encoding.UTF8.GetBytes(serialized));
            await _api.UploadFile(_manifestFileName, stream);
        }

        public static async Task AddSteamGuardAccountInGoogleDrive(string fileName, string filePath)
        {
            if (_api is null || _manifest is null)
                throw new ArgumentNullException();

            await using FileStream stream = new(filePath, FileMode.Open);
            using StreamReader reader = new(stream);
            await using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(await reader.ReadToEndAsync()));
            await _api.UploadFile(fileName, memoryStream);

            await GetAccountsFromGoogleDrive();
        }

        public static async Task GetAccountsFromGoogleDrive()
        {
            if (_api is null || _manifest is null)
                throw new ArgumentNullException();

            if (await _api.GetFiles() is not { } files)
            {
                files = new Google.Apis.Drive.v3.Data.File[0];
            }

            _manifest.Accounts.Clear();
            string downloadedData = string.Empty;
            GoogleDriveFileDownloader downloader = new(_api);
            downloader.OnDataDownloaded += (sender, args) =>
            {
                downloadedData = (string)args.UserState!;
            };

            foreach (var file in files)
            {
                if (!file.Name.Contains(".maFile")) continue;

                await downloader.Download(file);

                while (string.IsNullOrEmpty(downloadedData))
                {
                    await Task.Delay(10);
                }

                if (JsonConvert.DeserializeObject<SteamGuardAccount>(downloadedData) is not { } account)
                    throw new ArgumentNullException(nameof(account));

                _manifest.Accounts.Add(account);
            }
        }

        public static async Task DeleteSteamGuardAccountFromGoogleDrive(SteamGuardAccount account)
        {
            if (_manifest is null || _api is null)
                throw new ArgumentNullException();

            _manifest.Accounts.Remove(account);

            if (await FindMaFileInGoogleDrive(account) is { } file)
            {
                await _api.DeleteFile(file.Id);
            }
        }

        public static async Task SaveAccountInGoogleDrive(SteamGuardAccount account)
        {
            string serialized = JsonConvert.SerializeObject(account);

            if (await FindMaFileInGoogleDrive(account) is { } file)
            {
                await _api!.UploadFile(file, new MemoryStream(Encoding.UTF8.GetBytes(serialized)));
            }
        }

        #region PrivateFields

        private static async Task CreateNewManifestInGoogleDrive()
        {
            _manifest = new ManifestModel(true);
            await SaveManifestInGoogleFile();
        }

        private static async Task<GoogleFile?> FindMaFileInGoogleDrive(SteamGuardAccount account)
        {
            if (await _api!.GetFiles() is not { } files) return null;

            foreach (var file in files)
            {
                if (!file.Name.Contains(".maFile")) continue;
                string downloadedData = string.Empty;

                GoogleDriveFileDownloader downloader = new(_api);
                downloader.OnDataDownloaded += (sender, args) =>
                {
                    downloadedData = (string)args.UserState!;
                };
                await downloader.Download(file.Name);

                while (string.IsNullOrEmpty(downloadedData))
                {
                    await Task.Delay(10);
                }

                if (JsonConvert.DeserializeObject<SteamGuardAccount>(downloadedData) is not { } account2)
                    throw new ArgumentNullException(nameof(account2));

                if (account.Secret1 == account2.Secret1 && account.IdentitySecret == account2.IdentitySecret)
                {
                    return file;
                }
            }

            return null;
        }

        #endregion
    }
}
