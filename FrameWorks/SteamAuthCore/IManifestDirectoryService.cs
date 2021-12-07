namespace SteamAuthCore
{
    public interface IManifestDirectoryService
    {
        public string MaFilesDirectory { get; }
        public string ManifestFilePath { get; }

        public void CheckAndCreateDirectory();
    }
}
