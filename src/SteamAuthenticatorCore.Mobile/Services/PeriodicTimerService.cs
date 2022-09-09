using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class PeriodicTimerService : IPlatformTimer
{
    private CancellationTokenSource? _cts;
    private Func<CancellationToken, ValueTask> _func = null!;
    private Task? _timerTask;
    private PeriodicTimer? _timer;
    private bool _isDisposed;

    public void Initialize(TimeSpan interval, Func<CancellationToken, ValueTask> func)
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(interval);
        _func = func;
        _isDisposed = false;
    }

    public void Start()
    {
        if (_timer is null)
            return;

        _timerTask = DoWordAsync(_func);
    }

    public void Stop() => Dispose();

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _timer?.Dispose();
    }

    private async Task DoWordAsync(Func<CancellationToken, ValueTask> func)
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync(_cts?.Token ?? CancellationToken.None).ConfigureAwait(false))
            {
                await func.Invoke(_cts?.Token ?? CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }
}