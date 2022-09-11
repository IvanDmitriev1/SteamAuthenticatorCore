﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

internal sealed class BackgroundValueTaskService : BackgroundTaskServiceBase, IValueTaskTimer
{
    private Func<CancellationToken, ValueTask> _func = null!;

    public async ValueTask StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func)
    {
        await Initialize(timeSpan).ConfigureAwait(false);

        _func = func;
        TimerTask = DoWordAsync();
    }

    protected async override Task DoWordAsync()
    {
        try
        {
            while (await PeriodicTimer!.WaitForNextTickAsync(Cts.Token).ConfigureAwait(false))
            {
                await _func.Invoke(Cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
