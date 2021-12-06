using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SteamAuthCore
{
    public class LocalDriveManifestModelService : IManifestModelService
    {
        private ManifestModel _manifestModel = null!;
        private static readonly string MaFilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "maFiles");
        private static readonly string ManifestFilePath = Path.Combine(MaFilesDirectory, ManifestModelServiceConstants.ManifestFileName);

        public async Task Initialize()
        {
            if (!Directory.Exists(MaFilesDirectory))
                Directory.CreateDirectory(MaFilesDirectory);

            if (!File.Exists(ManifestFilePath))
            {
                _manifestModel = new ManifestModel();
                return;
            }

            using (var fileStream = new FileStream(ManifestFilePath, FileMode.Open, FileAccess.Read))
            {
                if (await JsonSerializer.DeserializeAsync<ManifestModel>(fileStream) is not { } manifest)
                    manifest = new ManifestModel();

                _manifestModel = manifest;
            }

            await SaveManifest();
        }

        public ManifestModel GetManifestModel()
        {
            return _manifestModel;
        }

        public async Task SaveManifest()
        {
            string serialized = JsonSerializer.Serialize(_manifestModel);

            using var fileStream = new FileStream(ManifestFilePath, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(serialized);
        }

        public async Task<ICollection<SteamGuardAccount>> GetAccounts()
        {
            if (_manifestModel.Accounts is not null)
                return _manifestModel.Accounts;

            var accounts = new List<SteamGuardAccount>();

            foreach (var file in Directory.GetFiles(MaFilesDirectory))
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
            string newFilePath = Path.Combine(MaFilesDirectory, fileName);

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

        private static async Task<string?> FindFileInDrive(SteamGuardAccount account)
        {
            var files = Directory.GetFiles(MaFilesDirectory);
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
