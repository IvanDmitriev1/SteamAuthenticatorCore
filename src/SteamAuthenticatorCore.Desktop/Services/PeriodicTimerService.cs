using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class PeriodicTimerService : IPlatformTimer
{
    private Task? _timerTask;
    private PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();

    public void Start(TimeSpan interval, Func<CancellationToken, ValueTask> func)
    {
        _timer = new PeriodicTimer(interval);
        _timerTask = DoWordAsync(func);
    }

    public void Stop()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_timerTask is null)
            return;

        _cts.Cancel();
        _cts.Dispose();
        _timer.Dispose();
    }

    private async Task DoWordAsync(Func<CancellationToken, ValueTask> func)
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync(_cts.Token))
            {
                await func.Invoke(_cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }
}