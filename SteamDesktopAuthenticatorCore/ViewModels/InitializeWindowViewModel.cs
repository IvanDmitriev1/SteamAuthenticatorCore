using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAuthCore;
using SteamDesktopAuthenticatorCore.classes;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class InitializeWindowViewModel : BaseViewModel
    {
        public InitializeWindowViewModel()
        {
            _manifestModelService = App.ManifestModelService;

            _buttonDelay = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _buttonDelay.Tick += (sender, args) =>
            {
                _refreshButtonClick = true;
                _buttonDelay.Stop();
            };
            _buttonDelay.Start();

            DispatcherTimer timer = new()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (sender, args) =>
            {
                if (ProgressBar >= 100) ProgressBar = 0;

                ProgressBar += 10;
            };
            timer.Start();
        }

        #region Variables

        private readonly IManifestModelService _manifestModelService;
        private Window _thisWindow = null!;
        private readonly DispatcherTimer _buttonDelay;
        private bool _refreshButtonClick;
        private int _progressBar;
        private bool _closeWindow;
        private string _text = string.Empty;
        private bool _windowOpened;

        #endregion

        #region Fields

        public int ProgressBar
        {
            get => _progressBar;
            set => Set(ref _progressBar, value);
        }

        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }

        #endregion

        #region Commands

        public ICommand WindowOnCloseCommand => new RelayCommand(o =>
        {
            if (!_closeWindow)
            {
                Application.Current.Shutdown();
            }
        });

        public ICommand WindowLoadedCommand => new AsyncRelayCommand(async o =>
        {
            if (o is not RoutedEventArgs { Source: Window window }) return;
            _thisWindow = window;

            await Init();
        });

        public ICommand RefreshCommand => new RelayCommand(async o =>
        {
            _refreshButtonClick = false;
            _buttonDelay.Start();

            await Init();
        }, o => _refreshButtonClick);

        #endregion

        #region PrivateMethods

        private async Task Init()
        {
            var settings = Settings.GetSettings();

            switch (settings.ManifestLocation)
            {
                case Settings.ManifestLocationModel.Drive:
                {
                    await StartWindows(settings);
                    return;
                }
                case Settings.ManifestLocationModel.GoogleDrive:
                {
                    try
                    {
                        await GoogleDriveSetup();
                    }
                    catch (HttpRequestException)
                    {
                        if (!App.GoogleDriveApi.IsAuthenticated)
                            MessageBox.Show($"Internet error occurred, restarting google drive auth");

                        RefreshCommand.Execute(null);
                    }
                    catch
                    {
                        //
                    }

                    if (!App.GoogleDriveApi.IsAuthenticated || _windowOpened) return;

                    await StartWindows(settings);

                    return;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task GoogleDriveSetup()
        {
            if (!await App.GoogleDriveApi.Init(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!))
            {
                await App.GoogleDriveApi.ConnectGoogleDrive(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!);
            }
        }

        private async Task StartWindows(Settings settings)
        {
            await App.InitializeManifestService();

            _refreshButtonClick = false;
            _windowOpened = true;

            if (settings.FirstRun)
            {
                settings.FirstRun = false;
                settings.SaveSettings();

                MainWindowView mainWindow = new();
                mainWindow.Show();
            }
            else
            {
                MainWindowView mainWindow = new();
                mainWindow.Show();
            }

            _closeWindow = true;
            _thisWindow.Close();
        }

        #endregion

    }
}
