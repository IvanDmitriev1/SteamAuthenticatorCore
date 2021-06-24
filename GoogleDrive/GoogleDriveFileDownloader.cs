using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace GoogleDrive
{
    public class GoogleDriveFileDownloader
    {
        public GoogleDriveFileDownloader(in GoogleDriveApi api)
        {
            _api = api;
        }

        #region Event

        public event AsyncCompletedEventHandler? OnDataDownloaded;

        private void OnOnDataDownloaded(AsyncCompletedEventArgs e)
        {
            OnDataDownloaded?.Invoke(this, e);
        }

        #endregion

        #region Varibales

        private readonly GoogleDriveApi _api;

        #endregion

        public async Task Download(string fileName)
        {
            if (await _api.CheckForFile(fileName) is not { } file) return;

            await Download(file);
        }

        public async Task Download(GoogleFile file)
        {
            _api.Downloaded += DataDownloaded;
            await _api.DownloadFile(file.Id, file.Name);
        }

        private async void DataDownloaded(string filename, MemoryStream stream)
        {
            using StreamReader reader = new(stream);
            string data = await reader.ReadToEndAsync();

            OnOnDataDownloaded(new AsyncCompletedEventArgs(null, false, data));

            await stream.DisposeAsync();
            _api.Downloaded -= DataDownloaded;
        }
    }
}