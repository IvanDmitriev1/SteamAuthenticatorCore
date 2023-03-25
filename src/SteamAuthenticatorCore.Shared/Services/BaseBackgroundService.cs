using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;

namespace SteamAuthenticatorCore.Shared.Services;

internal abstract class BaseBackgroundService : IAsyncDisposable
{
    protected CancellationTokenSource? Cts;
    protected PeriodicTimer? PeriodicTimer;
    protected Task? TimerTask;

    public async ValueTask DisposeAsync()
    {
        if (TimerTask is null)
            return;

        Debug.WriteLine($"On DisposeAsync: {GetType()}");

        Cts?.Cancel();
        await TimerTask.ConfigureAwait(false);
        PeriodicTimer?.Dispose();
        Cts?.Dispose();
    }

    protected async ValueTask Initialize(TimeSpan timeSpan)
    {
        await DisposeAsync().ConfigureAwait(false);

        PeriodicTimer = new PeriodicTimer(timeSpan);
        Cts = new CancellationTokenSource();

        Debug.WriteLine($"On Initialize: {GetType()}");
    }

    public async Task Stop()
    {
        await DisposeAsync().ConfigureAwait(false);
    }

    protected abstract Task DoWordAsync();
}
