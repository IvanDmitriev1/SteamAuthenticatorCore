using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Services;

internal class BackgroundTimer : BaseBackgroundTimer
{
    public BackgroundTimer(Action<CancellationToken> func)
    {
        _func = func;
    }

    private readonly Action<CancellationToken> _func;

    protected override async Task DoWordAsync()
    {
        try
        {
            while (!Cts.IsCancellationRequested && await PeriodicTimer.WaitForNextTickAsync(Cts.Token))
            {
                _func.Invoke(Cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}