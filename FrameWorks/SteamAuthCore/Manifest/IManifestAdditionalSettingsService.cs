using System.Threading.Tasks;

namespace SteamAuthCore.Manifest
{
    public class ManifestAdditionalSettings
    {
        public enum ManifestLocationModel
        {
            Drive,
            GoogleDrive
        }

        public ManifestLocationModel ManifestLocation { get; set; }
        public bool FirstRun { get; set; }
        public bool Updated { get; set; }
    }

    public interface IManifestAdditionalSettingsService
    {
        public Task Initialize();

        public ManifestAdditionalSettings GetSettings();
        public void SaveSettings(ManifestModel model);
    }
}
