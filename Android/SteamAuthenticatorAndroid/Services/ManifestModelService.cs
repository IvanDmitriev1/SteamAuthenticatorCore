using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SteamAuthCore;
using Xamarin.Essentials;

namespace SteamAuthenticatorAndroid.Services
{
    public static class ManifestModelService
    {
        static ManifestModelService()
        {
            ManifestFileName = Path.Combine(FileSystem.AppDataDirectory, "manifest.json");
        }

        private static ManifestModel? _manifest;
        private static readonly string ManifestFileName;

        public static async Task<ManifestModel> GetManifest()
        {
            if (_manifest is not null)
                return _manifest;

            if (!File.Exists(ManifestFileName))
            {
                await CreateNewManifest();
                return _manifest!;
            }

            await using FileStream stream = new(ManifestFileName, FileMode.Open);
            using StreamReader reader = new(stream);
            if (JsonSerializer.Deserialize<ManifestModel>(await reader.ReadToEndAsync()) is not { } manifest)
            {
                await CreateNewManifest();
                return _manifest!;
            }


            _manifest = manifest;
            return _manifest;
        }

        public static async Task SaveManifest()
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            string serialized = JsonSerializer.Serialize(_manifest);
            await File.WriteAllTextAsync(ManifestFileName, serialized);
        }

        public static async Task AddSteamGuardAccount(string filePath)
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            await using FileStream stream = new(filePath, FileMode.Open);
            using StreamReader reader = new(stream);
            if (JsonSerializer.Deserialize<SteamGuardAccount>(await reader.ReadToEndAsync()) is not { } account)
                throw new ArgumentNullException(nameof(account));

            _manifest.Accounts.Add(account);

            await SaveManifest();
        }

        public static async Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            _manifest.Accounts.Remove(account);

            await SaveManifest();
        }

        #region PrivateFields

        private static async Task CreateNewManifest()
        {
            ManifestModel manifest = new()
            {
                FirstRun = true,
            };

            _manifest = manifest;

            await SaveManifest();
        }

        #endregion
    }
}
