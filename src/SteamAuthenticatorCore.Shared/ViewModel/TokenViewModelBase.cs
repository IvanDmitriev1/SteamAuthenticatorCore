using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class TokenViewModelBase : ObservableObject
{
    protected TokenViewModelBase(ObservableCollection<SteamGuardAccount> accounts, ITimer timer, IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService, AccountsFileServiceResolver accountsFileServiceResolver)
    {
        PlatformImplementations = platformImplementations;
        AccountService = accountService;
        AccountsFileServiceResolver = accountsFileServiceResolver;
        Accounts = accounts;
        _token = string.Empty;
        
        timer.StartOrRestart(TimeSpan.FromSeconds(2), OnTimer);
    }

    protected readonly AccountsFileServiceResolver AccountsFileServiceResolver;
    protected readonly IPlatformImplementations PlatformImplementations;
    protected readonly ISteamGuardAccountService AccountService;
    private Int64 _currentSteamChunk;

    #region Propertis

    [ObservableProperty]
    private string _token;

    [ObservableProperty]
    private double _tokenProgressBar;

    [ObservableProperty]
    private SteamGuardAccount? _selectedAccount;

    public ObservableCollection<SteamGuardAccount> Accounts { get; }
    public bool IsMobile { get; set; }

    #endregion

    protected async Task RefreshAccountsSession(SteamGuardAccount account)
    {
        if (!await AccountService.RefreshSession(account, CancellationToken.None))
        {
            await PlatformImplementations.DisplayAlert("Refresh session", "Failed to refresh session");
            return;
        }

        await AccountsFileServiceResolver.Invoke().SaveAccount(account);
        await PlatformImplementations.DisplayAlert("Refresh session", "Session has been refreshed");
    }

    protected async ValueTask DeleteAccount(SteamGuardAccount account)
    {
        if (!await PlatformImplementations.DisplayPrompt("Delete account", "Are you sure?", "Yes", "No"))
            return;

        await AccountsFileServiceResolver.Invoke().DeleteAccount(account);
    }

    private ValueTask OnTimer(CancellationToken arg)
    {
        if (SelectedAccount is null)
            return ValueTask.CompletedTask;
        
        var steamTime = ITimeAligner.SteamTime;
        _currentSteamChunk = steamTime / 30L;
        var secondsUntilChange = (int)(steamTime - (_currentSteamChunk * 30L));

        if (steamTime != 0)
        {
            if (SelectedAccount.GenerateSteamGuardCode() is { } token)
                Token = token;
        }

        TokenProgressBar = 30 - secondsUntilChange;

        if (IsMobile)
            TokenProgressBar /= 30;

        return ValueTask.CompletedTask;
    }
}
