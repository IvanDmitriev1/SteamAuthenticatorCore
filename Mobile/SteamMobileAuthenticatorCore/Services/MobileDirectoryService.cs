using System.IO;
using SteamAuthCore;
using Xamarin.Essentials;

namespace SteamMobileAuthenticatorCore.Services
{
    public class MobileDirectoryService : IManifestDirectoryService
    {
        public MobileDirectoryService()
        {
            MaFilesDirectory = FileSystem.AppDataDirectory;
            ManifestFilePath = Path.Combine(MaFilesDirectory, ManifestModelServiceConstants.ManifestFileName);
        }

        public string MaFilesDirectory { get; }
        public string ManifestFilePath { get; }

        public void CheckAndCreateDirectory()
        {

        }
    }
}
