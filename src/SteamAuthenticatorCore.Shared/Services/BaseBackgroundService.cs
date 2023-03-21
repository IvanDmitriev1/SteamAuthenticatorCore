using System.Threading.Tasks;
using System.Threading;
using System;

namespace SteamAuthenticatorCore.Shared.Services;

internal abstract class BaseBackgroundService : IAsyncDisposable
{
    protected CancellationTokenSource Cts = null!;
    protected PeriodicTimer PeriodicTimer = null!;
    protected Task? TimerTask;

    public async ValueTask DisposeAsync()
    {
        if (TimerTask is null)
            return;

        Cts.Cancel();
        await TimerTask.ConfigureAwait(false);
        PeriodicTimer.Dispose();
        Cts.Dispose();
    }

    protected async ValueTask Initialize(TimeSpan timeSpan)
    {
        await DisposeAsync().ConfigureAwait(false);

        PeriodicTimer = new PeriodicTimer(timeSpan);
        Cts = new CancellationTokenSource();
    }

    public async Task Stop()
    {
        await DisposeAsync().ConfigureAwait(false);
    }

    protected abstract Task DoWordAsync();
}
