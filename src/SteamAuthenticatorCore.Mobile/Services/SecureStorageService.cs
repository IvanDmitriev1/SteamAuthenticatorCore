using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services;

public class SecureStorageService : IAccountsFileService
{
    public SecureStorageService(ObservableCollection<SteamGuardAccount> accounts)
    {
        _accounts = accounts;
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private readonly List<string> _fileNames = new();
    private const string Key = "AccountsNames";
    private bool _isInitialized;

    public async ValueTask InitializeOrRefreshAccounts()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            _fileNames.AddRange(GetFileNames());
        }

        _accounts.Clear();

        foreach (var accountsName in _fileNames)
        {
            if (await GetFromSecureStorage<SteamGuardAccount>(accountsName) is not { } account)
                continue;

            _accounts.Add(account);
        }
    }

    public async ValueTask<bool> SaveAccount(Stream stream, string fileName)
    {
        if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream) is not { } account)
            return false;

        stream.Seek(0, SeekOrigin.Begin);

        using var streamReader = new StreamReader(stream);
        var json = await streamReader.ReadToEndAsync();
        await SecureStorage.SetAsync(account.AccountName, json);

        _fileNames.Add(account.AccountName);
        Preferences.Set(Key, JsonSerializer.Serialize(_fileNames));

        _accounts.Add(account);
        return true;
    }

    public async ValueTask SaveAccount(SteamGuardAccount account)
    {
        var json = JsonSerializer.Serialize(account);
        await SecureStorage.SetAsync(account.AccountName, json);
    }

    public ValueTask DeleteAccount(SteamGuardAccount accountToRemove)
    {
        _fileNames.Remove(accountToRemove.AccountName);
        Preferences.Set(Key, JsonSerializer.Serialize(_fileNames));

        SecureStorage.Remove(accountToRemove.AccountName);
        _accounts.Remove(accountToRemove);
        return new ValueTask(Task.CompletedTask);
    }

    private static string[] GetFileNames()
    {
        if (Preferences.Get(Key, string.Empty) is not { } json)
            return Array.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    private static async ValueTask<T?> GetFromSecureStorage<T>(string key)
    {
        try
        {
            var json = await SecureStorage.GetAsync(key).ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception)
        {
            return default;
        }
    }
}