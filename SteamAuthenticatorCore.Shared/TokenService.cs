using System;
using System.Threading.Tasks;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared;

public sealed class TokenService : BaseViewModel
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
    private int _tokenProgressBar;
    private IPlatformTimer _timer;

    public string Token
    {
        get => _token;
        set => Set(ref _token, value);
    }

    public int TokenProgressBar
    {
        get => _tokenProgressBar;
        set => Set(ref _tokenProgressBar, value);
    }

    public SteamGuardAccount? SelectedAccount { get; set; }

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
    }
}