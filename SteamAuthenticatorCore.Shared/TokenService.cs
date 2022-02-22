using System;
using System.Threading.Tasks;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared;

public sealed class TokenService : BaseViewModel, IDisposable
{
    public TokenService(IPlatformTimer timer)
    {
        _timer = timer;
        _timer.ChangeInterval(TimeSpan.FromSeconds(2));
        _timer.SetFuncOnTick(SteamGuardTimerOnTick);
        _timer.Start();
    }

    private Int64 _currentSteamChunk;
    private Int64 _steamTime;

    private string _token = "Login token";
    private double _tokenProgressBar;
    private readonly IPlatformTimer _timer;

    public bool IsMobile { get; set; }

    public string Token
    {
        get => _token;
        set => Set(ref _token, value);
    }

    public double TokenProgressBar
    {
        get => _tokenProgressBar;
        set => Set(ref _tokenProgressBar, value);
    }

    public SteamGuardAccount? SelectedAccount { get; set; }

    public void Dispose()
    {
        _timer.Stop();
    }

    private async Task SteamGuardTimerOnTick()
    {
        _steamTime = await TimeAligner.GetSteamTimeAsync();
        _currentSteamChunk = _steamTime / 30L;
        int secondsUntilChange = (int)(_steamTime - (_currentSteamChunk * 30L));

        if (SelectedAccount is not null && _steamTime != 0)
        {
            if (SelectedAccount.GenerateSteamGuardCode(_steamTime) is { } token)
                Token = token;
        }

        if (SelectedAccount is not null)
            TokenProgressBar = 30 - secondsUntilChange;

        if (IsMobile)
            TokenProgressBar /= 30;
    }
}