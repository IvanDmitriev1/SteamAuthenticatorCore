using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GoogleDrive;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class GoogleDriveAccountsFileService : IAccountsFileService
{
    public GoogleDriveAccountsFileService(ObservableCollection<SteamGuardAccount> accounts, GoogleDriveApi api)
    {
        _accounts = accounts;
        _api = api;
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private readonly GoogleDriveApi _api;
    private bool _isInitialized;

    public async ValueTask InitializeOrRefreshAccounts()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            await Initialize();
        }

        if (await _api.GetFiles() is not { } files)
            files = Array.Empty<GoogleFile>();

        _accounts.Clear();

        foreach (var file in files)
        {
            try
            {
                if (!file.Name.Contains(IAccountsFileService.AccountFileExtension))
                    continue;

                if (await ApiDownload<SteamGuardAccount>(file.Id) is not { } account)
                    continue;

                _accounts.Add(account);
            }
            catch (Exception)
            {
                //
            }
        }
    }

    public async ValueTask<bool> SaveAccount(Stream stream, string fileName)
    {
        if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream) is not { } account)
            return false;

        stream.Seek(0, SeekOrigin.Begin);
        await _api.UploadFile(fileName, stream);

        _accounts.Add(account);
        return true;
    }

    public ValueTask SaveAccount(SteamGuardAccount account) => throw new NotImplementedException();

    public async ValueTask DeleteAccount(SteamGuardAccount accountToRemove)
    {
        if (await FindMaFileInGoogleDrive(accountToRemove) is not { } file)
            return;

        await _api.DeleteFile(file.Id);
        _accounts.Remove(accountToRemove);
    }

    private async ValueTask Initialize()
    {
        var appName = Assembly.GetEntryAssembly()!.GetName().Name;

        if (!await _api.Init(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{appName}.client_secret.json")!).ConfigureAwait(false))
            await _api.ConnectGoogleDrive(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{appName}.client_secret.json")!).ConfigureAwait(false);
    }

    private async ValueTask<T?> ApiDownload<T>(string id)
    {
        await using var stream = await _api.DownloadFileAsStream(id).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
    }

    private async ValueTask<GoogleFile?> FindMaFileInGoogleDrive(SteamGuardAccount accountToRemove)
    {
        if (await _api.GetFiles().ConfigureAwait(false) is not { } files)
            return null;

        var cts = new CancellationTokenSource();
        GoogleFile? foundedFile = null;

        try
        {
            await Parallel.ForEachAsync(files, cts.Token, async (file, token) =>
            {
                if (!file.Name.Contains(IAccountsFileService.AccountFileExtension))
                    return;

                if (await ApiDownload<SteamGuardAccount>(file.Id).ConfigureAwait(false) is not { } account)
                    return;

                if (accountToRemove.Secret1 != account.Secret1 || accountToRemove.IdentitySecret != account.IdentitySecret)
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