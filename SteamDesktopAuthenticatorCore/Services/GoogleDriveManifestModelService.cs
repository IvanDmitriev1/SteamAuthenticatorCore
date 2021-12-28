using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GoogleDrive;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace SteamDesktopAuthenticatorCore.Services
{
    public class GoogleDriveManifestModelService : IManifestModelService
    {
        public GoogleDriveManifestModelService(GoogleDriveApi api)
        {
            _api = api;
        }

        private readonly GoogleDriveApi _api;
        private ManifestModel _manifestModel = null!;


        public async Task Initialize()
        {
            if (!await _api.Init(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!))
                await _api.ConnectGoogleDrive(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!);

            if (await _api.CheckForFile(ManifestModelServiceConstants.ManifestFileName) is not { } manifestFile)
            {
                _manifestModel = new ManifestModel();
                return;
            }

            if (await ApiDownload<ManifestModel>(manifestFile.Id) is not { } manifest)
                throw new ArgumentException(nameof(manifest));

            _manifestModel = manifest;
        }

        public ManifestModel GetManifestModel()
        {
            return _manifestModel;
        }

        public async Task SaveManifest()
        {
            string serialized = JsonSerializer.Serialize(_manifestModel);
            await using MemoryStream stream = new(Encoding.UTF8.GetBytes(serialized));
            await _api.UploadFile(ManifestModelServiceConstants.ManifestFileName, stream);
        }

        public async Task<ICollection<SteamGuardAccount>> GetAccounts()
        {
            if (await _api.GetFiles() is not { } files)
            {
                files = Array.Empty<GoogleFile>();
            }

            List<SteamGuardAccount> accounts = new List<SteamGuardAccount>();

            foreach (var file in files)
            {
                if (!file.Name.Contains(".maFile")) continue;

                if (await ApiDownload<SteamGuardAccount>(file.Id) is not { } account)
                    continue;

                accounts.Add(account);
            }

            return accounts;
        }

        public async Task<SteamGuardAccount?> AddSteamGuardAccount(Stream fileStream, string fileName)
        {
            await _api.UploadFile(fileName, fileStream);
            return await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream);
        }

        public async Task SaveSteamGuardAccount(SteamGuardAccount account)
        {
            string serialized = JsonSerializer.Serialize(_manifestModel);

            if (await FindMaFileInGoogleDrive(account) is { } file)
            {
                await using MemoryStream stream = new(Encoding.UTF8.GetBytes(serialized));
                await _api.UploadFile(file, new MemoryStream(Encoding.UTF8.GetBytes(serialized)));
            }
        }

        public async Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            if (await FindMaFileInGoogleDrive(account) is { } file)
                await _api.DeleteFile(file.Id);
        }

        private async Task<T?> ApiDownload<T>(string id)
        {
            await using var stream = await _api.DownloadFileAsStream(id);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        private async Task<GoogleFile?> FindMaFileInGoogleDrive(SteamGuardAccount account)
        {
            if (await _api.GetFiles() is not { } files) return null;
            foreach (var file in files)
            {
                if (!file.Name.Contains(".maFile")) continue;
                if (JsonSerializer.Deserialize<SteamGuardAccount>(await _api.DownloadFileAsString(file.Id)) is not { } account2)
                    return default;

                if (account.Secret1 == account2.Secret1 && account.IdentitySecret == account2.IdentitySecret)
                    return file;
            }

            return default;
        }
    }
}
