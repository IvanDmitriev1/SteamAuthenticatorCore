using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Models;
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
            if (await SettingsModelService.GetSettingsModel() is not { } settings)
            {
                ManifestModel manifest = await ManifestModelService.GetManifestFromDrive();
                StartWindows(ref manifest);
                return;
            }

            switch (settings.ManifestLocation)
            {
                case ManifestLocation.Drive:
                {
                    ManifestModel manifest = await ManifestModelService.GetManifestFromDrive();
                    StartWindows(ref manifest);
                    return;
                }
                case ManifestLocation.GoogleDrive:
                {
                    ManifestModel manifest = new();

                    try
                    {
                        manifest = await GoogleDriveSetup();
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

                    if (settings.ImportFiles)
                    {
                        await ImportFilesToGoogleDrive();
                        settings.ImportFiles = false;

                        await SettingsModelService.SaveSettings();
                    }

                    StartWindows(ref manifest);

                    return;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task<ManifestModel> GoogleDriveSetup()
        {
            if (!await App.GoogleDriveApi.Init(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!))
            {
                await App.GoogleDriveApi.ConnectGoogleDrive(Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamDesktopAuthenticatorCore.client_secret.json")!);
            }

            return await ManifestModelService.GetManifestFromGoogleDrive();
        }

        private static async Task ImportFilesToGoogleDrive()
        {
            foreach (var file in Directory.GetFiles(ManifestModelService.MaFilesDirectory))
            {
                if (!file.Contains(".maFile")) continue;

                try
                {
                    await using FileStream fileStream = new(file, FileMode.Open);
                    using StreamReader reader = new(fileStream);
                    await ManifestModelService.AddSteamGuardAccountInGoogleDrive(Path.GetFileName(file), await reader.ReadToEndAsync());
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            await ManifestModelService.GetAccountsInGoogleDrive();
        }

        private void StartWindows(ref ManifestModel manifest)
        {
            _refreshButtonClick = false;
            _windowOpened = true;

            if (manifest.FirstRun)
            {
                if (manifest.Accounts.Count == 0)
                {
                    WelcomeWindowView welcomeWindow = new();
                    welcomeWindow.Show();

                    _closeWindow = true;
                    _thisWindow.Close();
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
