using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopTimer : IPlatformTimer
{
    public DesktopTimer()
    {
        _timer = new DispatcherTimer();
    }

    private readonly DispatcherTimer _timer;
    private Action? _actionCallback;
    private Func<Task>? _taskCallback;

    public void ChangeInterval(TimeSpan time)
    {
        _timer.Interval = time;
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
        _timer.Start();

        if (_actionCallback is not null)
        {
            _timer.Tick += (sender, args) =>
            {
                _actionCallback!.Invoke();
            };

            return;
        }

        _timer.Tick += async (sender, args) =>
        {
            await _taskCallback!.Invoke();
        };
    }

    public void Stop()
    {
        _timer.Stop();
    }
}