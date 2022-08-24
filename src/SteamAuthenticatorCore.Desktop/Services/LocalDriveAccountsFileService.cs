﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class LocalDriveAccountsFileService : IAccountsFileService
{
    public LocalDriveAccountsFileService(ObservableCollection<SteamGuardAccount> accounts)
    {
        _accounts = accounts;
        _maFilesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SteamAuthenticatorCore.Desktop");
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private readonly string _maFilesDirectory;

    public async ValueTask InitializeOrRefreshAccounts()
    {
        CreateDirectory();

        _accounts.Clear();

        foreach (var filePath in Directory.GetFiles(_maFilesDirectory))
        {
            if (!filePath.Contains(IAccountsFileService.AccountFileExtension))
                continue;

            await using var fileStream = File.OpenRead(filePath);

            if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream) is not { } account)
                continue;

            _accounts.Add(account);
        }
    }

    public async ValueTask<bool> SaveAccount(Stream stream, string fileName)
    {
        if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream) is not { } account)
            return false;

        var newFileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileName), IAccountsFileService.AccountFileExtension);
        var newFilePath = Path.Combine(_maFilesDirectory, newFileName);

        stream.Seek(0, SeekOrigin.Begin);
        await using var newFileStream = File.OpenWrite(newFilePath);
        await stream.CopyToAsync(newFileStream);


        _accounts.Add(account);
        return true;
    }

    public async ValueTask SaveAccount(SteamGuardAccount account)
    {
        if (await FindAccountInDirectory(account) is not { } filePath)
            return;

        var json = JsonSerializer.Serialize(account);
        await using var fileStream = File.OpenWrite(filePath);
        await using var streamWriter = new StreamWriter(fileStream);
        await streamWriter.WriteAsync(json);
    }

    public async ValueTask DeleteAccount(SteamGuardAccount accountToRemove)
    {
        if (await FindAccountInDirectory(accountToRemove) is not { } filePath)
            return;

        File.Delete(filePath);
        _accounts.Remove(accountToRemove);
    }

    private void CreateDirectory()
    {
        if (!Directory.Exists(_maFilesDirectory))
            Directory.CreateDirectory(_maFilesDirectory);
    }

    private async ValueTask<string?> FindAccountInDirectory(SteamGuardAccount accountToFind)
    {
        var cts = new CancellationTokenSource();
        string? foundedFilePath = null;

        try
        {
            await Parallel.ForEachAsync(Directory.GetFiles(_maFilesDirectory), cts.Token, async (filePath, token) =>
            {
                if (!filePath.Contains(IAccountsFileService.AccountFileExtension))
                    return;

                await using var fileStream = File.OpenRead(filePath);
                if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream, cancellationToken: token) is not { } account)
                    return;

                if (account.Secret1 != accountToFind.Secret1 || account.AccountName != accountToFind.AccountName)
                    return;

                foundedFilePath = filePath;
                cts.Cancel();
            });
        }
        catch (OperationCanceledException)
        {
            
        }

        return foundedFilePath;
    }
}