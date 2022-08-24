using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthCore.Abstractions;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class TokenViewModelBase : ObservableObject
{
    protected TokenViewModelBase(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, IPlatformImplementations platformImplementations, ITimeAligner timeAligner, ISteamGuardAccountService accountService)
    {
        _platformImplementations = platformImplementations;
        _timeAligner = timeAligner;
        _accountService = accountService;
        Accounts = accounts;
        _token = string.Empty;
        
        platformTimer.Initialize(TimeSpan.FromSeconds(2), OnTimer);
        platformTimer.Start();
    }

    private readonly IPlatformImplementations _platformImplementations;
    private readonly ITimeAligner _timeAligner;
    private readonly ISteamGuardAccountService _accountService;
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

    #region Commands

    [RelayCommand]
    private async Task LoginInSelectedAccount()
    {
        if (SelectedAccount is null)
            return;

        if (await _accountService.RefreshSession(SelectedAccount))
        {
            await _platformImplementations.DisplayAlert("Your session has been refreshed.");
            return;
        }

        await _platformImplementations.DisplayAlert("Failed to refresh your session.\nTry using the \"Login again\" option.");
    }

    #endregion

    private ValueTask OnTimer(CancellationToken arg)
    {
        if (SelectedAccount is null)
            return new ValueTask(Task.CompletedTask);

        var steamTime = _timeAligner.SteamTime;
        _currentSteamChunk = steamTime / 30L;
        var secondsUntilChange = (int)(steamTime - (_currentSteamChunk * 30L));

        if (steamTime != 0)
        {
            if (SelectedAccount.GenerateSteamGuardCode(steamTime) is { } token)
                Token = token;
        }

        TokenProgressBar = 30 - secondsUntilChange;

        if (IsMobile)
            TokenProgressBar /= 30;

        return new ValueTask(Task.CompletedTask);
    }
}
