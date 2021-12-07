using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.Helpers
{
    public class Timer
    {
        public Timer(Action callback)
        {
            _actionCallback = callback;
        }

        public Timer(Func<Task> callback)
        {
            _taskCallback = callback;
        }

        public bool IsRunning { get; private set; }
        private TimeSpan _interval;

        private readonly Action? _actionCallback;
        private readonly Func<Task>? _taskCallback;

        public void Start(TimeSpan interval)
        {
            IsRunning = true;
            _interval = interval;

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
}
