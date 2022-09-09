using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

public class BackgroundTaskService : ITimer
{
    private PeriodicTimer? _periodicTimer;
    private CancellationTokenSource? _cts;
    private Task? _timerTask;
    private Func<CancellationToken, ValueTask>? _func;

    public void StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func)
    {
        _periodicTimer = new PeriodicTimer(timeSpan);
        _func = func;
        _cts = new CancellationTokenSource();

        _timerTask = DoWordAsync();
    }

    public async ValueTask StopAsync()
    {
        if (_timerTask is null)
            return;

        _cts!.Cancel();
        await _timerTask.ConfigureAwait(false);
        _cts!.Dispose();
        _periodicTimer!.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private async Task DoWordAsync()
    {
        try
        {
            while (await _periodicTimer!.WaitForNextTickAsync(_cts!.Token).ConfigureAwait(false))
            {
                await _func!.Invoke(_cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
