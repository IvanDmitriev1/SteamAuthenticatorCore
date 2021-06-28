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
        public static GoogleDriveApi? Api { get; set; }

        public static async Task<ManifestModel> GetManifestFromGoogleDrive()
        {
            if (_manifest is not null)
                return _manifest;

            if (Api is null)
                throw new ArgumentNullException(nameof(Api));

            if (await Api.CheckForFile(ManifestFileName) is not { } manifestFile)
            {
                await CreateNewManifestInGoogleDrive();
                return _manifest!;
            }

            GoogleDriveFileDownloader downloader = new(Api);
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

            await GetAccountsInGoogleDrive();
            return _manifest;
        }

        public static async Task SaveManifestInGoogleFile()
        {
            if (Api is null || _manifest is null)
                throw new ArgumentNullException();

            ManifestModel newModel = new(_manifest);

            string serialized = JsonConvert.SerializeObject(newModel);
            await using MemoryStream stream = new(Encoding.UTF8.GetBytes(serialized));
            await Api.UploadFile(ManifestFileName, stream);
        }

        public static async Task AddSteamGuardAccountInGoogleDrive(string fileName, string filePath)
        {
            if (Api is null || _manifest is null)
                throw new ArgumentNullException();

            await using FileStream stream = new(filePath, FileMode.Open);
            using StreamReader reader = new(stream);
            await using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(await reader.ReadToEndAsync()));
            await Api.UploadFile(fileName, memoryStream);

            await GetAccountsInGoogleDrive();
        }

        public static async Task GetAccountsInGoogleDrive()
        {
            if (Api is null || _manifest is null)
                throw new ArgumentNullException();

            if (await Api.GetFiles() is not { } files)
            {
                files = new GoogleFile[0];
            }

            _manifest.Accounts.Clear();
            string downloadedData = string.Empty;
            GoogleDriveFileDownloader downloader = new(Api);
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

        public static async Task DeleteSteamGuardAccountInGoogleDrive(SteamGuardAccount account)
        {
            if (_manifest is null || Api is null)
                throw new ArgumentNullException();

            _manifest.Accounts.Remove(account);

            if (await FindMaFileInGoogleDrive(account) is { } file)
            {
                await Api.DeleteFile(file.Id);
            }
        }

        public static async Task SaveAccountInGoogleDrive(SteamGuardAccount account)
        {
            string serialized = JsonConvert.SerializeObject(account);

            if (await FindMaFileInGoogleDrive(account) is { } file)
            {
                await Api!.UploadFile(file, new MemoryStream(Encoding.UTF8.GetBytes(serialized)));
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
            if (await Api!.GetFiles() is not { } files) return null;

            foreach (var file in files)
            {
                if (!file.Name.Contains(".maFile")) continue;
                string downloadedData = string.Empty;

                GoogleDriveFileDownloader downloader = new(Api);
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
                    return file;
            }

            return null;
        }

        #endregion
    }
}
