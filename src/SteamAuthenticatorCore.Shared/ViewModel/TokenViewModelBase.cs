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
    protected TokenViewModelBase(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer)
    {
        Accounts = accounts;
        _token = string.Empty;
        
        platformTimer.Initialize(TimeSpan.FromSeconds(2), OnTimer);
        platformTimer.Start();
    }

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

    private ValueTask OnTimer(CancellationToken arg)
    {
        if (SelectedAccount is null)
            return new ValueTask(Task.CompletedTask);
        
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

        return new ValueTask(Task.CompletedTask);
    }
}
