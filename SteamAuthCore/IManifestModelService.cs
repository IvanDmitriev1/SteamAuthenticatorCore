using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SteamAuthCore
{
    public interface IManifestModelService
    {
        public const string FileExtension = ".maFile";
        protected const string ManifestFileName = "manifest.json";

        public Task Initialize();

        public ManifestModel GetManifestModel();
        public Task SaveManifest();

        public Task<ICollection<SteamGuardAccount>> GetAccounts();
        public Task<SteamGuardAccount?> AddSteamGuardAccount(FileStream fileStream);
        public Task SaveSteamGuardAccount(SteamGuardAccount account);
        public Task DeleteSteamGuardAccount(SteamGuardAccount account);
    }
}
