using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SteamAuthCore.Manifest
{
    public static class ManifestModelServiceConstants
    {
        public const string FileExtension = ".maFile";
        public const string ManifestFileName = "manifest.json";
    }

    public interface IManifestModelService
    {
        public Task Initialize(IManifestDirectoryService? directoryService = null);

        public ManifestModel GetManifestModel();
        public Task SaveManifest();

        public Task<ICollection<SteamGuardAccount>> GetAccounts();
        public Task<SteamGuardAccount?> AddSteamGuardAccount(Stream fileStream, string fileName);
        public Task SaveSteamGuardAccount(SteamGuardAccount account);
        public Task DeleteSteamGuardAccount(SteamGuardAccount account);
    }
}
