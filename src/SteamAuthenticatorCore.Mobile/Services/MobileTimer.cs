using System;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstractions;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileTimer : IPlatformTimer
{
    private bool _isRunning;
    private TimeSpan _interval;
    private Func<CancellationToken, ValueTask>? _func;
    private CancellationTokenSource? _cts;

    public void Initialize(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func)
    {
        _interval = timeSpan;
        _func = func;
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _isRunning = false;
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Start()
    {
        if (_func is null)
            throw new NullReferenceException("Timer is not initialized");

        _isRunning = true; 
        Device.StartTimer(_interval, Callback);
    }

    private bool Callback()
    {
        HelpMethod();
        return _isRunning;
    }

    public void Stop() => Dispose();

    private async void HelpMethod()
    {
        try
        {
            await _func!.Invoke(_cts!.Token);
        }
        catch (OperationCanceledException)
        {
            
        }
    }
}