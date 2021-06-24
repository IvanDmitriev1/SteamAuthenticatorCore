using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuthCore.Models;
using Xamarin.Essentials;

namespace SteamDesktopAuthenticatorCoreAndroid.Services
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
            if (JsonConvert.DeserializeObject<ManifestModel>(reader.ReadToEnd()) is not { } manifest)
                throw new ArgumentNullException(nameof(manifest));

            _manifest = manifest;
            return _manifest;
        }

        public static async Task SaveManifest()
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            string serialized = JsonConvert.SerializeObject(_manifest);
            await File.WriteAllTextAsync(ManifestFileName, serialized);
        }

        public static async Task AddSteamGuardAccount(string filePath)
        {
            if (_manifest is null)
                throw new ArgumentNullException();

            await using FileStream stream = new(filePath, FileMode.Open);
            using StreamReader reader = new(stream);
            if (JsonConvert.DeserializeObject<SteamGuardAccount>(await reader.ReadToEndAsync()) is not { } account)
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
