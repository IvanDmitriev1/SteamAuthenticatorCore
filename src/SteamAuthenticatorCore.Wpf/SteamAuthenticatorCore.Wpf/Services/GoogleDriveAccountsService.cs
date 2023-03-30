using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class GoogleDriveAccountsService : IAccountsService
{
    public GoogleDriveAccountsService()
    {
        _accounts = new List<SteamGuardAccount>();
        
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolderPath = Path.Combine(appDataPath, App.Name);
        var userCredentialPath = Path.Combine(appFolderPath, "Token.json");

        _api = new GoogleDriveApi(userCredentialPath, new[] { Google.Apis.Drive.v3.DriveService.Scope.DriveFile }, App.Name);
    }

    private readonly List<SteamGuardAccount> _accounts;
    private readonly GoogleDriveApi _api;
    private bool _isInitialized;

    public async Task Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
            
        var appName = Assembly.GetEntryAssembly()!.GetName().Name;

        if (!await _api.Init(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{appName}.client_secret.json")!).ConfigureAwait(false))
            await _api.ConnectGoogleDrive(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{appName}.client_secret.json")!).ConfigureAwait(false);

        if (await _api.GetFiles().ConfigureAwait(false) is not { } files)
            files = Array.Empty<GoogleFile>();

        _accounts.Clear();

        foreach (var file in files)
        {
            try
            {
                if (!file.Name.Contains(IAccountsService.AccountFileExtension))
                    continue;

                if (await ApiDownload<SteamGuardAccount>(file.Id).ConfigureAwait(false) is not { } account)
                    continue;

                _accounts.Add(account);
            }
            catch (Exception)
            {
                //
            }
        }
    }

    public ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll()
    {
        IReadOnlyList<SteamGuardAccount> wrapper = _accounts.AsReadOnly();
        return ValueTask.FromResult(wrapper);
    }

    public async ValueTask<bool> Save(Stream stream, string fileName)
    {
        try
        {
            if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream) is not { } account)
                return false;

            stream.Seek(0, SeekOrigin.Begin);
            await _api.UploadFile(fileName, stream).ConfigureAwait(false);

            _accounts.Add(account);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async ValueTask Update(SteamGuardAccount account)
    {
        if (await FindMaFileInGoogleDrive(account).ConfigureAwait(false) is not { } file)
            return;

        var json = JsonSerializer.Serialize(account);

        await using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        await _api.UploadFile(file, stream).ConfigureAwait(false);
    }

    public async ValueTask Delete(SteamGuardAccount account)
    {
        if (await FindMaFileInGoogleDrive(account).ConfigureAwait(false) is not { } file)
            return;

        await _api.DeleteFile(file.Id).ConfigureAwait(false);
        _accounts.Remove(account);
    }

    private async ValueTask<T?> ApiDownload<T>(string id)
    {
        await using var stream = await _api.DownloadFileAsStream(id).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
    }

    private async ValueTask<GoogleFile?> FindMaFileInGoogleDrive(SteamGuardAccount accountToFind)
    {
        if (await _api.GetFiles().ConfigureAwait(false) is not { } files)
            return null;

        var cts = new CancellationTokenSource();
        GoogleFile? foundedFile = null;

        try
        {
            await Parallel.ForEachAsync(files, cts.Token, async (file, token) =>
            {
                if (!file.Name.Contains(IAccountsService.AccountFileExtension))
                    return;

                if (await ApiDownload<SteamGuardAccount>(file.Id).ConfigureAwait(false) is not { } account)
                    return;

                if (accountToFind.Secret1 != account.Secret1 || accountToFind.IdentitySecret != account.IdentitySecret)
                    return;

                foundedFile = file;
                cts.Cancel();
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            
        }

        return foundedFile;
    }
}