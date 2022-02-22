using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileTimer : IPlatformTimer
{
    public MobileTimer()
    {

    }

    public bool IsRunning { get; private set; }
    private TimeSpan _interval;

    private Action? _actionCallback;
    private Func<Task>? _taskCallback;


    public void ChangeInterval(TimeSpan time)
    {
        _interval = time;
    }

    public void SetFuncOnTick(Func<Task> callback)
    {
        _taskCallback = callback;
    }

    public void SetFuncOnTick(Action callback)
    {
        _actionCallback = callback;
    }

    public void Start()
    {
        IsRunning = true;

        if (_actionCallback is not null)
        {
            Device.StartTimer(_interval, ActionCallback);
            return;
        }

        Device.StartTimer(_interval, FuncCallback);
    }

    public void Stop()
    {
        IsRunning = false;
    }

    private bool ActionCallback()
    {
        _actionCallback!.Invoke();

        return IsRunning;
    }

    private bool FuncCallback()
    {
        HelpMethod();
        return IsRunning;
    }

    private async void HelpMethod()
    {
        await _taskCallback!.Invoke();
    }
}