using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SteamAuthCore
{
    public class DesktopManifestDirectoryService : IManifestDirectoryService
    {
        public DesktopManifestDirectoryService()
        {
            MaFilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "maFiles");
            ManifestFilePath = Path.Combine(MaFilesDirectory, ManifestModelServiceConstants.ManifestFileName);
        }

        public string MaFilesDirectory { get; }
        public string ManifestFilePath { get; }

        public void CheckAndCreateDirectory()
        {
            if (!Directory.Exists(MaFilesDirectory))
                Directory.CreateDirectory(MaFilesDirectory);
        }
    }

    public class LocalDriveManifestModelService : IManifestModelService
    {
        public LocalDriveManifestModelService(IManifestDirectoryService directoryService)
        {
            _manifestDirectoryService = directoryService;
        }

        private readonly IManifestDirectoryService _manifestDirectoryService;
        private ManifestModel? _manifestModel;

        public async Task Initialize()
        {
            _manifestDirectoryService.CheckAndCreateDirectory();

            if (!File.Exists(_manifestDirectoryService.ManifestFilePath))
            {
                _manifestModel = new ManifestModel();
                return;
            }

            using (var fileStream = new FileStream(_manifestDirectoryService.ManifestFilePath, FileMode.Open, FileAccess.Read))
            {
                if (await JsonSerializer.DeserializeAsync<ManifestModel>(fileStream) is not { } manifest)
                    manifest = new ManifestModel();

                _manifestModel = manifest;
            }

            await SaveManifest();
        }

        public ManifestModel GetManifestModel()
        {
            return _manifestModel!;
        }

        public async Task SaveManifest()
        {
            string serialized = JsonSerializer.Serialize(_manifestModel);

            using var fileStream = new FileStream(_manifestDirectoryService.ManifestFilePath, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(serialized);
        }

        public async Task<ICollection<SteamGuardAccount>> GetAccounts()
        {
            if (_manifestModel is null)
                await Initialize();

            if (_manifestModel!.Accounts is not null)
                return _manifestModel.Accounts;

            var accounts = new List<SteamGuardAccount>();

            foreach (var file in Directory.GetFiles(_manifestDirectoryService.MaFilesDirectory))
            {
                if (!file.Contains(ManifestModelServiceConstants.FileExtension)) continue;

                using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

                if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream) is not { } account)
                    continue;

                accounts.Add(account);
            }

            return accounts;
        }

        public async Task<SteamGuardAccount?> AddSteamGuardAccount(FileStream fileStream)
        {
            var account = await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream);

            string fileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileStream.Name), ".maFile");
            string newFilePath = Path.Combine(_manifestDirectoryService.MaFilesDirectory, fileName);

            fileStream.Seek(0, SeekOrigin.Begin);

            using FileStream newFileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(newFileStream);
            await newFileStream.FlushAsync();

            return account;
        }

        public async Task SaveSteamGuardAccount(SteamGuardAccount account)
        {
            if (await FindFileInDrive(account) is not { } file)
                return;

            string serialized = JsonSerializer.Serialize(account);
            using var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(serialized);
        }

        public async Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            if (await FindFileInDrive(account) is { } path)
                File.Delete(path);
        }

        private async Task<string?> FindFileInDrive(SteamGuardAccount account)
        {
            var files = Directory.GetFiles(_manifestDirectoryService.MaFilesDirectory);
            foreach (var file in files)
            {
                if (!file.Contains(ManifestModelServiceConstants.FileExtension)) continue;

                using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

                if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream) is not { } account2)
                    continue;

                if (account.Secret1 == account2.Secret1 && account.IdentitySecret == account2.IdentitySecret)
                    return file;
            }

            return null;
        }
    }
}
