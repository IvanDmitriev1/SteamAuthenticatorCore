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

namespace SteamAuthenticatorCore.Desktop.Services;

public sealed class GoogleDriveApi : IDisposable
{
    public GoogleDriveApi(in string userCredentialPath, in string[] scopes, string appName)
    {
        _userCredentialPath = userCredentialPath;
        _scopes = scopes;
        _appName = appName;
    }

    private string? _folderId;
    private readonly string _appName;
    private readonly string[] _scopes;
    private readonly string _userCredentialPath;
    private DriveService? _googleDriveService;
    private UserCredential? _userCredential;

    #region Fields

    public bool IsAuthenticated { get; private set; }
    private DriveService GoogleDriveService
    {
        get
        {
            if (_googleDriveService is null)
                throw new ArgumentNullException(nameof(GoogleDriveService));

            return _googleDriveService;
        }
        set => _googleDriveService = value;
    }

    private string FolderId
    {
        get
        {
            if (_folderId is null)
                throw new ArgumentNullException(nameof(FolderId));

            return _folderId;
        }
        set => _folderId = value;
    }

    #endregion

    public async Task<bool> Init(Stream clientSecretStream)
    {
        if (!Directory.Exists(_userCredentialPath))
            return false;

        await ConnectGoogleDrive(clientSecretStream).ConfigureAwait(false);
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

        GoogleDriveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = _userCredential,
            ApplicationName = _appName
        });

        if (await CheckForFile(_appName) is { } file)
            FolderId = file.Id;
        else
            FolderId = await CreateAppFolder(_appName);

        IsAuthenticated = true;
    }

    public async Task<string> UploadFile(string fileName, Stream stream)
    {
        //updating previous data
        if (await CheckForFile(fileName) is { } file)
            return await UploadFile(file, stream);

        string contentType = $"file/{Path.GetExtension(fileName).Remove(0, 1)}";
        GoogleFile fileMetadata = new()
        {
            Name = fileName,
            MimeType = "file",
            Parents = new List<string> {FolderId}
        };

        //Creating new file
        FilesResource.CreateMediaUpload createRequest = GoogleDriveService.Files.Create(fileMetadata, stream, contentType);
        await createRequest.UploadAsync();
        return createRequest.ResponseBody.Id;
    }

    public async Task<string> UploadFile(GoogleFile file, Stream stream)
    {
        string contentType = $"file/{Path.GetExtension(file.Name).Remove(0, 1)}";
        GoogleFile fileMetadata = new()
        {
            Name = file.Name
        };

        //updating previous data
        FilesResource.UpdateMediaUpload updateRequest = GoogleDriveService.Files.Update(fileMetadata, file.Id, stream, contentType);
        await updateRequest.UploadAsync();
        return updateRequest.ResponseBody.Id;
    }

    public async Task<MemoryStream> DownloadFileAsStream(string fileId)
    {
        FilesResource.GetRequest? request = GoogleDriveService.Files.Get(fileId);
        MemoryStream stream = new();

        request.MediaDownloader.ProgressChanged += progress =>
        {
            switch (progress.Status)
            {
                case DownloadStatus.Downloading:
                {
                    break;
                }
                case DownloadStatus.Completed:
                {
                    byte[] bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF8, stream.ToArray());
                    stream = new MemoryStream(bytes);

                    break;
                }
                case DownloadStatus.Failed:
                    throw new Exception("Failed to download");
                case DownloadStatus.NotStarted:
                    throw new Exception("Download not started");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };

        await request.DownloadAsync(stream);
        return stream;
    }

    public async Task<string> DownloadFileAsString(string fileId)
    {
        await using MemoryStream stream = await DownloadFileAsStream(fileId);
        using StreamReader reader = new(stream);

        return await reader.ReadToEndAsync();
    }

    public async Task<GoogleFile[]?> GetFiles()
    {
        // Define parameters of request.
        FilesResource.ListRequest listRequest = GoogleDriveService.Files.List();
        listRequest.Fields = "nextPageToken, files(id, name, parents)";

        // List files.
        return (await listRequest.ExecuteAsync()).Files.ToArray();
    }

    public async Task<GoogleFile?> CheckForFile(string? fileName)
    {
        if (await GetFiles() is not { } files) return null;
        return files.FirstOrDefault(data => data.Name == Path.GetFileName(fileName));
    }

    public async Task DeleteFile(string fileId)
    {
        await GoogleDriveService.Files.Delete(fileId).ExecuteAsync();
    }

    public async Task RenameFile(string oldFileName, string newFileName)
    {
        if (await CheckForFile($"{oldFileName}.pm") is { Id: { } id})
        {
            GoogleFile fileMetadata = new()
            {
                Name = $"{newFileName}.pm"
            };

            await GoogleDriveService.Files.Update(fileMetadata, id).ExecuteAsync();
        }
    }

    #region PrivateMethods

    private async Task<string> CreateAppFolder(string folderName)
    {
        GoogleFile fileMetadata = new()
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder"
        };

        FilesResource.CreateRequest? request = GoogleDriveService.Files.Create(fileMetadata);
        request!.Fields = "id";

        GoogleFile file = await request.ExecuteAsync();
        return file.Id;
    }

    #endregion

    public void Dispose()
    {
        GoogleDriveService.Dispose();
    }
}