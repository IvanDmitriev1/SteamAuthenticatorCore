using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

internal abstract class BaseBackgroundTimer : IBackgroundTimer
{
    public bool IsRunning { get; private set; }

    protected CancellationTokenSource Cts = null!;
    protected PeriodicTimer PeriodicTimer = null!;
    private Task? _timerTask;

    public async ValueTask StartOrRestart(TimeSpan timeSpan)
    {
        if (!IsRunning)
        {
            Start(timeSpan);
            return;
        }

        await DisposeAsync();
        Start(timeSpan);
    }

    public void Start(TimeSpan timeSpan)
    {
        PeriodicTimer = new PeriodicTimer(timeSpan);
        Cts = new CancellationTokenSource();

        _timerTask = DoWordAsync();
        IsRunning = true;

        Debug.WriteLine("Initializing timer");
    }

    public async ValueTask Stop()
    {
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_timerTask is null)
            return;

        Debug.WriteLine("Started to disposing timer");

        Cts.Cancel();
        await _timerTask;

        Cts.Dispose();
        PeriodicTimer.Dispose();
        IsRunning = false;
        _timerTask = null;

        Debug.WriteLine("Finished disposing of the timer");
    }

    protected abstract Task DoWordAsync();
}
