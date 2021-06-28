using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuthCore.Models;

namespace SteamDesktopAuthenticatorCore.Services
{
    public static partial class ManifestModelService
    {
        static ManifestModelService()
        {
            MaFilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "maFiles");

            ManifestFilePath = Path.Combine(MaFilesDirectory, ManifestFileName);
        }

        private static readonly string ManifestFilePath;
        private static readonly string MaFilesDirectory;

        public static async Task<ManifestModel> GetManifestFromDrive()
        {
            if (_manifest is not null)
                return _manifest;

            if (!Directory.Exists(MaFilesDirectory))
                Directory.CreateDirectory(MaFilesDirectory);

            if (!File.Exists(ManifestFilePath))
            {
                await CreateNewManifestInDrive();
                return _manifest!;
            }

            if (JsonConvert.DeserializeObject<ManifestModel>(await File.ReadAllTextAsync(ManifestFilePath)) is not { } manifest)
                throw new ArgumentNullException(nameof(manifest));

            _manifest = manifest;

            await GetAccountsInDrive();

            return _manifest;
        }

        public static async Task SaveManifestInDrive()
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            ManifestModel newModel = new(_manifest);

            string serialized = JsonConvert.SerializeObject(newModel);
            await File.WriteAllTextAsync(ManifestFilePath, serialized);
        }

        public static async Task GetAccountsInDrive()
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            _manifest.Accounts.Clear();

            string[] files = Directory.GetFiles(MaFilesDirectory);
            foreach (var file in files)
            {
                if (!file.Contains(".maFile")) continue;

                if (JsonConvert.DeserializeObject<SteamGuardAccount>(await File.ReadAllTextAsync(file)) is not { } account)
                    throw new ArgumentNullException(nameof(account));

                _manifest.Accounts.Add(account);
            }
        }

        public static async Task AddSteamGuardAccountInDrive(string fileName, string filePath)
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            await using FileStream stream = new(filePath, FileMode.Open);
            using StreamReader reader = new(stream);
            string data = await reader.ReadToEndAsync();

            if (JsonConvert.DeserializeObject<SteamGuardAccount>(data) is not { } account)
                throw new ArgumentNullException(nameof(account));

            await File.WriteAllTextAsync(Path.Combine(MaFilesDirectory, fileName), data);

            _manifest.Accounts.Add(account);
            await SaveManifestInDrive();
        }

        public static async Task DeleteSteamGuardAccountInDrive(SteamGuardAccount account)
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            _manifest.Accounts.Remove(account);

            if (await FindFileInDrive(account) is { } file)
                File.Delete(file);
        }

        #region PrivateMethods

        private static async Task CreateNewManifestInDrive()
        {
            _manifest = new ManifestModel(true);
            await SaveManifestInDrive();
        }

        private static async Task<string?> FindFileInDrive(SteamGuardAccount account)
        {
            string[] files = Directory.GetFiles(MaFilesDirectory);
            foreach (var file in files)
            {
                if (!file.Contains(".maFile")) continue;

                if (JsonConvert.DeserializeObject<SteamGuardAccount>(await File.ReadAllTextAsync(file)) is not { } account2)
                    throw new ArgumentNullException(nameof(account));

                if (account.Secret1 == account2.Secret1 && account.IdentitySecret == account2.IdentitySecret)
                    return file;
            }

            return null;
        }

        #endregion
    }
}
