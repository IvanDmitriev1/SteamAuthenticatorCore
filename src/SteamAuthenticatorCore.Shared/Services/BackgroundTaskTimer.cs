using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Services;

internal sealed class BackgroundTaskTimer : BaseBackgroundTimer
{
    public BackgroundTaskTimer(Func<CancellationToken, Task> func)
    {
        _func = func;
    }

    private readonly Func<CancellationToken, Task> _func;

    protected override async Task DoWordAsync()
    {
        try
        {
            while (!Cts.IsCancellationRequested && await PeriodicTimer.WaitForNextTickAsync(Cts.Token))
            {
                await _func.Invoke(Cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
