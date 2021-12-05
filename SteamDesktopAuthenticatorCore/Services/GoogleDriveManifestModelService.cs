using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GoogleDrive;
using SteamAuthCore;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace SteamDesktopAuthenticatorCore.Services
{
    public class GoogleDriveManifestModelService : IManifestModelService
    {
        public GoogleDriveManifestModelService()
        {
            _api = App.GoogleDriveApi;
            
        }

        private readonly GoogleDriveApi _api;
        private ManifestModel _manifestModel = null!;


        public async Task Initialize()
        {
            if (await _api.CheckForFile(IManifestModelService.ManifestFileName) is not { } manifestFile)
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

        public Task SaveManifest()
        {
            throw new NotImplementedException();
        }

        public Task DeleteManifest()
        {
            throw new NotImplementedException();
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

        public Task<SteamGuardAccount?> AddSteamGuardAccount(FileStream fileStream)
        {
            throw new NotImplementedException();
        }

        public Task SaveSteamGuardAccount(SteamGuardAccount account)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            throw new NotImplementedException();
        }

        private async Task<T?> ApiDownload<T>(string id)
        {
            using var stream = await _api.DownloadFileAsStream(id);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
    }
}
