using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

public class BackgroundTaskService : ITimer
{
    private Task? _timerTask;
    private PeriodicTimer _periodicTimer = null!;
    private CancellationTokenSource _cts = null!;
    private Func<CancellationToken, ValueTask> _func = null!;

    public async ValueTask StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func)
    {
        await DisposeAsync().ConfigureAwait(false);

        _periodicTimer = new PeriodicTimer(timeSpan);
        _func = func;
        _cts = new CancellationTokenSource();

        _timerTask = DoWordAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_timerTask is null)
            return;

        _cts.Cancel();
        await _timerTask.ConfigureAwait(false);
        _periodicTimer.Dispose();
        _cts.Dispose();
    }

    private async Task DoWordAsync()
    {
        try
        {
            while (await _periodicTimer!.WaitForNextTickAsync(_cts.Token).ConfigureAwait(false))
            {
                await _func.Invoke(_cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
