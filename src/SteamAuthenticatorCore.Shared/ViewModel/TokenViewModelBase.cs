using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class TokenViewModelBase : ObservableObject
{
    protected TokenViewModelBase(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, IPlatformImplementations platformImplementations)
    {
        _platformTimer = platformTimer;
        _platformImplementations = platformImplementations;
        Accounts = accounts;
        _token = string.Empty;
        
        _platformTimer.Initialize(TimeSpan.FromSeconds(2), OnTimer);
        _platformTimer.Start();
    }

    private readonly IPlatformTimer _platformTimer;
    private readonly IPlatformImplementations _platformImplementations;
    private Int64 _currentSteamChunk;
    private Int64 _steamTime;

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

        if (await SelectedAccount.RefreshSessionAsync())
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

        _steamTime = TimeAligner.GetSteamTime();
        _currentSteamChunk = _steamTime / 30L;
        var secondsUntilChange = (int)(_steamTime - (_currentSteamChunk * 30L));

        if (_steamTime != 0)
        {
            if (SelectedAccount.GenerateSteamGuardCode(_steamTime) is { } token)
                Token = token;
        }

        TokenProgressBar = 30 - secondsUntilChange;

        if (IsMobile)
            TokenProgressBar /= 30;

        return new ValueTask(Task.CompletedTask);
    }
}
