using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SteamAuthCore.Manifest
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

        private IManifestDirectoryService _manifestDirectoryService;
        private ManifestModel? _manifestModel;
        private bool _isInitialized;

        public async Task Initialize()
        {
            if (_isInitialized) return;

            _manifestDirectoryService.CheckAndCreateDirectory();

            if (!File.Exists(_manifestDirectoryService.ManifestFilePath))
            {
                _manifestModel = new ManifestModel();
                await File.Create(_manifestDirectoryService.ManifestFilePath).DisposeAsync();
            }

            ManifestModel model;
            await using (var stream = File.OpenRead(_manifestDirectoryService.ManifestFilePath))
            {
                try
                {
                    if (JsonSerializer.Deserialize<ManifestModel>(stream) is not { } manifest)
                        manifest = new ManifestModel();

                    model = manifest;
                }
                catch
                {
                    model = new ManifestModel();
                }
            }

            _manifestModel = model;

            await SaveManifest();
            _isInitialized = true;
        }

        public ManifestModel GetManifestModel() => _manifestModel!;

        public async Task SaveManifest()
        {
            string serialized = JsonSerializer.Serialize(_manifestModel);

            await using var fileStream = File.OpenWrite(_manifestDirectoryService.ManifestFilePath);
            await using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(serialized);
        }

        public async Task<ICollection<SteamGuardAccount>> GetAccounts()
        {
            if (_manifestModel!.Accounts is not null)
                return _manifestModel.Accounts;

            var accounts = new List<SteamGuardAccount>();

            foreach (var file in Directory.GetFiles(_manifestDirectoryService.MaFilesDirectory))
            {
                if (!file.Contains(ManifestModelServiceConstants.FileExtension)) continue;

                await using var fileStream = File.OpenRead(file);

                if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream) is not { } account)
                    continue;

                accounts.Add(account);
            }

            return accounts;
        }

        public async Task<SteamGuardAccount?> AddSteamGuardAccount(Stream fileStream, string fileName)
        {
            var account = await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream);

            string newFileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileName), ".maFile");
            string newFilePath = Path.Combine(_manifestDirectoryService.MaFilesDirectory, newFileName);

            fileStream.Seek(0, SeekOrigin.Begin);

            await using var newFileStream = File.OpenWrite(newFilePath);
            await fileStream.CopyToAsync(newFileStream);

            return account;
        }

        public async Task SaveSteamGuardAccount(SteamGuardAccount account)
        {
            if (await FindFileInDrive(account) is not { } file)
                return;

            string serialized = JsonSerializer.Serialize(account);
            await using var fileStream = File.OpenWrite(file);
            await using var streamWriter = new StreamWriter(fileStream);
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

                await using var fileStream = File.OpenRead(file);

                if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream) is not { } account2)
                    continue;

                if (account.Secret1 == account2.Secret1 && account.IdentitySecret == account2.IdentitySecret)
                    return file;
            }

            return null;
        }
    }
}
