using System.IO;
using SteamAuthCore.Manifest;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services
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
