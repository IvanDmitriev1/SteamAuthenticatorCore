using SteamAuthenticatorCore.Shared.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Services;

internal class BackgroundService : BaseBackgroundService, ITimer
{
    private Action<CancellationToken> _func = null!;

    public async Task StartOrRestart(TimeSpan timeSpan, Action<CancellationToken> func)
    {
        await Initialize(timeSpan);

        _func = func;
        TimerTask = DoWordAsync();
    }

    protected override async Task DoWordAsync()
    {
        try
        {
            while (await PeriodicTimer.WaitForNextTickAsync(Cts.Token).ConfigureAwait(false))
            {
                _func.Invoke(Cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}