using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.Services;

public class ManifestAccountsWatcherService
{
    public ManifestAccountsWatcherService(AppSettings settings, ManifestServiceResolver manifestServiceResolver, IPlatformImplementations platformImplementations, ObservableCollection<SteamGuardAccount> accounts)
    {
        _settings = settings;
        _manifestServiceResolver = manifestServiceResolver;
        _platformImplementations = platformImplementations;
        _accounts = accounts;
    }

    private readonly AppSettings _settings;
    private readonly ManifestServiceResolver _manifestServiceResolver;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private CancellationTokenSource _cts = new();

    public async ValueTask RefreshAccounts()
    {
        _accounts.Clear();

        foreach (var account in await _manifestServiceResolver.Invoke().GetAccounts().ConfigureAwait(false))
        {
            await _platformImplementations.InvokeMainThread(() =>
            {
                _accounts.Add(account);
            });
        }
    }

    public async ValueTask Initialize()
    {
        var manifestService = _manifestServiceResolver.Invoke();
        await manifestService.Initialize();

        try
        {
            _cts.Token.ThrowIfCancellationRequested();

            await RefreshAccounts();
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }
    }

    public async Task ImportSteamGuardAccounts(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            await ImportSteamGuardAccount(stream, stream.Name);
        }
    }

    public async Task ImportSteamGuardAccount(Stream stream, string fileName)
    {
        try
        {
            if (await _manifestServiceResolver.Invoke().AddSteamGuardAccount(stream, fileName) is { } account)
                _accounts.Add(account);
        }
        catch
        {
            await _platformImplementations.DisplayAlert("Your file is corrupted");
        }
    }

    public async ValueTask DeleteAccount(SteamGuardAccount account)
    {
        await _manifestServiceResolver.Invoke().DeleteSteamGuardAccount(account);
        _accounts.Remove(account);
    }
}
