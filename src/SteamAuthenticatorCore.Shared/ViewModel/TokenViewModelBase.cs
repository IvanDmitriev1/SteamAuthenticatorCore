using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class TokenViewModelBase : ObservableObject
{
    protected TokenViewModelBase(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer)
    {
        _platformTimer = platformTimer;
        Accounts = accounts;
        _token = string.Empty;
        _platformTimer.Start(TimeSpan.FromSeconds(2), OnTimer);
    }

    private readonly IPlatformTimer _platformTimer;
    private Int64 _currentSteamChunk;
    private Int64 _steamTime;

    [ObservableProperty]
    private string _token;

    [ObservableProperty]
    private double _tokenProgressBar;

    [ObservableProperty]
    private SteamGuardAccount? _selectedAccount;

    public ObservableCollection<SteamGuardAccount> Accounts { get; }
    public bool IsMobile { get; set; }

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
