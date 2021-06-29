namespace SteamDesktopAuthenticatorCore.Models
{
    public enum ManifestLocation
    {
        Drive,
        GoogleDrive
    }
    public class SettingsModel
    {
        public ManifestLocation ManifestLocation { get; set; }

        public bool ImportFiles { get; set; } = false;
    }
}
