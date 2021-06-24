using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace GoogleDrive
{
    public class GoogleDriveApi : IDisposable
    {
        public GoogleDriveApi(in string userCredentialPath, in string[] scopes, string appName)
        {
            _userCredentialPath = userCredentialPath;
            _scopes = scopes;
            _appName = appName;
        }

        #region Events

        public delegate void DownloadArgs(string fileName, MemoryStream stream);
        public event EventHandler? Connected;
        public event DownloadArgs? Downloaded;

        private void OnConnection()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }
        private void OnDownloaded(string fileName, in MemoryStream stream)
        {
            Downloaded?.Invoke(fileName, stream);
        }

        #endregion

        #region Variables

        private string? _folderId;
        private readonly string _appName;
        private readonly string[] _scopes;
        private readonly string _userCredentialPath;
        private DriveService? _googleDriveService;
        private UserCredential? _userCredential;

        #endregion

        #region Fields
        public bool IsAuthenticated { get; private set; }
        public bool IsDownloading { get; private set; }

        #endregion

        public async Task<bool> Init(Stream clientSecretStream)
        {
            if (!Directory.Exists(_userCredentialPath)) return false;

            await ConnectGoogleDrive(clientSecretStream);
            return true;
        }

        public async Task ConnectGoogleDrive(Stream clientSecretStream)
        {
            GoogleClientSecrets clientSecrets = await GoogleClientSecrets.FromStreamAsync(clientSecretStream);

            _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets.Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(_userCredentialPath, true));

            _googleDriveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _userCredential,
                ApplicationName = _appName
            });

            if (await CheckForFile(_appName) is { } file)
                _folderId = file.Id;
            else
                _folderId = await CreateAppFolder(_appName);

            IsAuthenticated = true;
            OnConnection();
        }

        public async Task<string?> UploadFile(string fileName, MemoryStream stream)
        {
            if (_googleDriveService is null || _folderId is null) return null;

            //updating previous data
            if (await CheckForFile(fileName) is { } file)
                return await UploadFile(file, stream);

            string contentType = $"file/{Path.GetExtension(fileName).Remove(0, 1)}";
            GoogleFile fileMetadata = new()
            {
                Name = fileName,
                MimeType = "file",
                Parents = new List<string> {_folderId}
            };

            //Creating new file
            FilesResource.CreateMediaUpload createRequest = _googleDriveService.Files.Create(fileMetadata, stream, contentType);
            await createRequest.UploadAsync();
            return createRequest.ResponseBody.Id;
        }

        public async Task<string?> UploadFile(GoogleFile file, MemoryStream stream)
        {
            if (_googleDriveService is null || _folderId is null) return null;

            string contentType = $"file/{Path.GetExtension(file.Name).Remove(0, 1)}";
            GoogleFile fileMetadata = new()
            {
                Name = file.Name
            };

            //updating previous data
            FilesResource.UpdateMediaUpload updateRequest = _googleDriveService.Files.Update(fileMetadata, file.Id, stream, contentType);
            await updateRequest.UploadAsync();
            return updateRequest.ResponseBody.Id;
        }

        public async Task DownloadFile(string fileId, string fileName)
        {
            if (_googleDriveService is null || _folderId is null) return;
            IsDownloading = true;

            FilesResource.GetRequest? request = _googleDriveService.Files.Get(fileId);
            MemoryStream stream = new();

            request.MediaDownloader.ProgressChanged += async progress =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                    {
                        break;
                    }
                    case DownloadStatus.Completed:
                    {
                        byte[] bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, stream.ToArray());

                        OnDownloaded(fileName, new MemoryStream(bytes));

                        await stream.DisposeAsync();
                        IsDownloading = false;
                        break;
                    }
                    case DownloadStatus.Failed:
                    {
                        break;
                    }
                    case DownloadStatus.NotStarted:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            await request.DownloadAsync(stream);
        }

        public async Task<GoogleFile[]?> GetFiles()
        {
            if (_googleDriveService is null) return null;

            // Define parameters of request.
            FilesResource.ListRequest listRequest = _googleDriveService.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name, parents)";

            // List files.
            IList<GoogleFile> driveData = (await listRequest.ExecuteAsync()).Files;
            return driveData.ToArray();
        }

        public async Task<GoogleFile?> CheckForFile(string? fileName)
        {
            if (_googleDriveService is null || fileName is null) return null;

            if (await GetFiles() is not { } files) return null;
            return files.FirstOrDefault(data => data.Name == Path.GetFileName(fileName));
        }

        public async Task DeleteFile(string fileId)
        {
            await _googleDriveService?.Files.Delete(fileId).ExecuteAsync()!;
        }

        public async Task RenameFile(string oldFileName, string newFileName)
        {
            if (_googleDriveService is null) return;

            if (await CheckForFile($"{oldFileName}.pm") is { Id: { } id})
            {
                GoogleFile fileMetadata = new()
                {
                    Name = $"{newFileName}.pm"
                };

                await _googleDriveService.Files.Update(fileMetadata, id).ExecuteAsync();
            }
        }

        #region PrivateMethods
        private async Task<string?> CreateAppFolder(string folderName)
        {
            if (_googleDriveService is null) return null;

            GoogleFile fileMetadata = new()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            FilesResource.CreateRequest? request = _googleDriveService.Files.Create(fileMetadata);
            request!.Fields = "id";

            GoogleFile file = await request.ExecuteAsync();
            return file.Id;
        }

        #endregion

        public void Dispose()
        {
            _googleDriveService?.Dispose();
        }
    }
}