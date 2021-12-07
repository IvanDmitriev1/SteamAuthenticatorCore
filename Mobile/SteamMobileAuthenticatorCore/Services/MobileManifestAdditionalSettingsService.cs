using System;
using System.Threading.Tasks;
using SteamAuthCore.Manifest;

namespace SteamMobileAuthenticatorCore.Services
{
    public class MobileManifestAdditionalSettingsService : IManifestAdditionalSettingsService
    {
        private ManifestAdditionalSettings _additionalSettings = null!;

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public ManifestAdditionalSettings GetSettings()
        {
            throw new NotImplementedException();
        }

        public void SaveSettings(ManifestModel model)
        {
            
        }
    }
}
