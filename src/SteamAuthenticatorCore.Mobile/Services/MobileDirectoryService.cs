using System.IO;
using SteamAuthCore.Manifest;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services
{
    public class MobileDirectoryService : IManifestDirectoryService
    {
        public MobileDirectoryService()
        {
            MaFilesDirectory = Path.Combine(FileSystem.AppDataDirectory, "MaFiles");
            ManifestFilePath = Path.Combine(FileSystem.AppDataDirectory, ManifestModelServiceConstants.ManifestFileName);
        }

        public string MaFilesDirectory { get; }
        public string ManifestFilePath { get; }

        public void CheckAndCreateDirectory()
        {
            if (!Directory.Exists(MaFilesDirectory))
                Directory.CreateDirectory(MaFilesDirectory);
        }
    }
}
