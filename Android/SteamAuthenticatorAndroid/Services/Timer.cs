using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorAndroid.Services
{
    class Timer
    {
        public Timer(int interval, Action method)
        {
            _method = method;
            _interval = interval;
        }

        private readonly Action _method;
        private int _interval;
        private bool _alive;
        public int Interval
        {
            get => _interval;
            set
            {
                Restart();
                _interval = value;
            }
        }

        public void Start()
        {
            if (_alive)
                return;

            _alive = true;

            Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(Interval), () =>
            {
                _method.BeginInvoke(_method.EndInvoke, null);

                return _alive;
            });
        }

        public void Stop()
        {
            _alive = false;
        }

        private async void Restart()
        {
            Stop();

            await Task.Delay(TimeSpan.FromSeconds(Interval + 2));

            Start();
        }
    }
}
