using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class InitializeWindowViewModel : BaseViewModel
    {
        public InitializeWindowViewModel()
        {
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

        private Window _thisWindow = null!;
        private readonly DispatcherTimer _buttonDelay;
        private bool _refreshButtonClick;
        private int _progressBar;
        private bool _closeWindow;

        #endregion

        #region Fields

        public int ProgressBar
        {
            get => _progressBar;
            set => Set(ref _progressBar, value);
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
            ManifestModel manifestModel = new();

            try
            {
                manifestModel = await GoogleDriveSetup();
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

            if (App.GoogleDriveApi.IsAuthenticated)
                StartWindows(ref manifestModel);
        }

        private static async Task<ManifestModel> GoogleDriveSetup()
        {
            if (!await App.GoogleDriveApi.Init(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!))
            {
                await App.GoogleDriveApi.ConnectGoogleDrive(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!);
            }

            return await ManifestModelService.GetManifestFromGoogleDrive(App.GoogleDriveApi);
        }

        private void StartWindows(ref ManifestModel manifest)
        {
            _refreshButtonClick = false;

            if (manifest.FirstRun)
            {
                if (manifest.Accounts.Count == 0)
                {
                    WelcomeWindowView welcomeWindow = new();
                    welcomeWindow.Show();
                    return;
                }

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
