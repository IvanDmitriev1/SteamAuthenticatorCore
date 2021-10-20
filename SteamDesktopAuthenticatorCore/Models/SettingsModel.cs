namespace SteamDesktopAuthenticatorCore.Models
{
    
    public class SettingsModel
    {
        public enum ManifestLocationModel
        {
            Drive,
            GoogleDrive
        }

        public ManifestLocationModel ManifestLocation { get; set; }
        public bool FirstRun { get; set; }

        public bool ImportFiles { get; set; } = false;
    }
}
