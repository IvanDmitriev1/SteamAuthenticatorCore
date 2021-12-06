using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SteamAuthCore
{
    public static class ManifestModelServiceConstants
    {
        public const string FileExtension = ".maFile";
        public const string ManifestFileName = "manifest.json";
    }

    public interface IManifestModelService
    {
        public Task Initialize();

        public ManifestModel GetManifestModel();
        public Task SaveManifest();

        public Task<ICollection<SteamGuardAccount>> GetAccounts();
        public Task<SteamGuardAccount?> AddSteamGuardAccount(FileStream fileStream);
        public Task SaveSteamGuardAccount(SteamGuardAccount account);
        public Task DeleteSteamGuardAccount(SteamGuardAccount account);
    }
}
