using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class PeriodicTimerService : IPlatformTimer
{
    private readonly CancellationTokenSource _cts = new();
    private Func<CancellationToken, ValueTask> _func;
    private Task? _timerTask;
    private PeriodicTimer? _timer;

    public void Initialize(TimeSpan interval, Func<CancellationToken, ValueTask> func)
    {
        _timer = new PeriodicTimer(interval);
        _func = func;
    }

    public void Start()
    {
        if (_timer is null)
            return;

        _timerTask = DoWordAsync(_func);
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
        _timer?.Dispose();
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