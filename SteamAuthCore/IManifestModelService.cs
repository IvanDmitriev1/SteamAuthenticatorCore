using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SteamAuthCore
{
    public interface IManifestModelService
    {
        protected const string ManifestFileName = "manifest.json";

        public Task Initialize();

        public ManifestModel GetManifestModel();
        public Task SaveManifest();
        public Task DeleteManifest();

        public Task<ICollection<SteamGuardAccount>> GetAccounts();
        public Task<SteamGuardAccount?> AddSteamGuardAccount(FileStream fileStream);
        public Task SaveSteamGuardAccount(SteamGuardAccount account);
        public Task DeleteSteamGuardAccount(SteamGuardAccount account);
    }
}
