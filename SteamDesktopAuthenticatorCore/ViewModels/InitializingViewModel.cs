using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAuthCore.Manifest;
using SteamDesktopAuthenticatorCore.Common;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper.Commands;
using WpfHelper.Common;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class InitializingViewModel : BaseViewModel
    {
        public InitializingViewModel(App.ManifestServiceResolver manifestServiceResolver, Container container, SettingService settingsService)
        {
            _manifestModelService = manifestServiceResolver.Invoke();
            _container = container;
            _settingsService = settingsService;

            _buttonDelay = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _buttonDelay.Tick += (sender, args) =>
            {
                _refreshButtonClick = true;
                _buttonDelay.Stop();
            };

            _progressBarTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _progressBarTimer.Tick += (sender, args) =>
            {
                if (ProgressBar >= 100) ProgressBar = 0;

                ProgressBar += 10;
            };
        }

        private readonly DispatcherTimer _buttonDelay;
        private readonly DispatcherTimer _progressBarTimer;
        private readonly IManifestModelService _manifestModelService;
        private readonly SettingService _settingsService;
        private Container? _container;
        private Window? _window;

        private bool _refreshButtonClick;
        private int _progressBar;

        public int ProgressBar
        {
            get => _progressBar;
            set => Set(ref _progressBar, value);
        }


        public ICommand WindowLoadedCommand => new AsyncRelayCommand(async o =>
        {
            _window = o as Window;

            _buttonDelay.Start();
            _progressBarTimer.Start();

            await InitializeManifest();
        });

        public ICommand RefreshCommand => new AsyncRelayCommand(async o =>
        {
            await InitializeManifest();
        }, o => _refreshButtonClick);

        private async Task InitializeManifest()
        {
            try
            {
                await _manifestModelService.Initialize();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            ShowWindow();
        }

        private void ShowWindow()
        {
            var appSettings = _settingsService.Get<AppSettings>();
            if (appSettings.FirstRun)
                appSettings.FirstRun = false;


            _container?.Show();
            _container = null;

            CloseEverything();
        }

        private void CloseEverything()
        {
            _buttonDelay.Stop();
            _progressBarTimer.Stop();

            _window?.Close();
        }
    }
}
