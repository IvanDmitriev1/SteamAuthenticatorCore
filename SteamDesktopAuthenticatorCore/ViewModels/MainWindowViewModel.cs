using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using SteamAuthCore;
using SteamDesktopAuthenticatorCore.classes;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;
using WpfHelper.Custom;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
        {
            if (!App.InDesignMode)
            {
                Manifest = ManifestModelService.GetManifest().Result;
                
                Task.Run(() =>
                {
                    var settings = Settings.GetSettings();

                    SwitchText = settings.ManifestLocation switch
                    {
                        Settings.ManifestLocationModel.Drive => "Switch to using the files on your Google Drive",
                        Settings.ManifestLocationModel.GoogleDrive => "Switch to using the files on your disk",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }); 
            }
            else
            {
                Manifest = new ManifestModel();
            }

            _selectedAccountFont = 12.0;
            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            _confirmationsWindow = new ViewConfirmationsWindowView();

            #region Timers

            _steamGuardTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1),
            };
            _steamGuardTimer.Tick += SteamGuardTimerOnTick;
            _steamGuardTimer.Start();

            _autoTradeConfirmationTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(Manifest.PeriodicCheckingInterval)
            };
            _autoTradeConfirmationTimer.Tick += async (sender, args) => await AutoTradeConfirmationTimerOnTick();

            if (Manifest.AutoConfirmMarketTransactions)
                _autoTradeConfirmationTimer.Start();

            #endregion
        }

        #region Variables

        private Window _thisWindow = null!;
        private Int64 _steamTime;
        private Int64 _currentSteamChunk;
        private readonly DispatcherTimer _steamGuardTimer;
        private readonly DispatcherTimer _autoTradeConfirmationTimer;
        private SteamGuardAccount? _selectedAccount;

        private double _selectedAccountFont;
        private string _loginTokenText = "Login token";
        private string _statusText = string.Empty;
        private int _progressBar;
        private string _switchText = string.Empty;

        private readonly ViewConfirmationsWindowView _confirmationsWindow;

        #endregion

        #region Fields

        public ManifestModel Manifest { get; set; }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (value is null)
                    return;

                switch (value.AccountName.Length)
                {
                    case < 39:
                        SelectedAccountFont = 12;
                        break;
                    case > 39:
                        SelectedAccountFont = 10;
                        break;
                }

                if (value.AccountName.Length > 46)
                    SelectedAccountFont = 8;

                var dataContext = (_confirmationsWindow.DataContext as ViewConfirmationsWindowViewModel)!;
                dataContext.SelectedAccount = value;

                Set(ref _selectedAccount, value, nameof(SelectedAccount), nameof(SelectedAccountFont));
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
        }

        public string LoginTokenText
        {
            get => _loginTokenText;
            set => Set(ref _loginTokenText, value);
        }

        public int ProgressBar
        {
            get => _progressBar;
            set => Set(ref _progressBar, value);
        }

        public string SwitchText
        {
            get => _switchText;
            set => Set(ref _switchText, value);
        }

        public double SelectedAccountFont
        {
            get => _selectedAccountFont;
            set => Set(ref _selectedAccountFont, value);
        }

        public string CurrentVersion { get; private set; }

        #endregion

        #region Commands

        public ICommand WindowOnLoadedCommand => new RelayCommand(o =>
        {
            if (o is not RoutedEventArgs { Source: Window window }) return;

            _thisWindow = window;
        });

        public ICommand WindowOnClosingCommand => new RelayCommand(o =>
        {
            _confirmationsWindow.Close();
        });

        public ICommand WindowCloseCommand => new RelayCommand(o =>
        {
            _thisWindow.Close();
            _confirmationsWindow.Close();
        });

        public ICommand TokenOnInputCommand => new RelayCommand(o =>
        {
            if (o is not TextCompositionEventArgs args) return;

            args.Handled = true;
        });

        public ICommand SwitchCommand => new RelayCommand(o =>
        {
            var settings = Settings.GetSettings();

            settings.ManifestLocation = settings.ManifestLocation switch
            {
                Settings.ManifestLocationModel.Drive => Settings.ManifestLocationModel.GoogleDrive,
                Settings.ManifestLocationModel.GoogleDrive => Settings.ManifestLocationModel.Drive,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (settings.ManifestLocation == Settings.ManifestLocationModel.GoogleDrive)
                if (CustomMessageBox.Show("Import your current files to google drive?", "Import service", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    settings.ImportFiles = true;

            settings.SaveSettings();

            CustomMessageBox.Show("Restart application");

            //Process.Start();
            Application.Current.Shutdown(0);
            
        });

        public ICommand ShowWindowCommand => new RelayCommand(o =>
        {
            _thisWindow.Show();

            if (_thisWindow.WindowState == WindowState.Minimized)
                _thisWindow.WindowState = WindowState.Normal;
        });

        public ICommand SettingsShowCommand => new RelayCommand(o =>
        {
            _steamGuardTimer.Stop();
            _autoTradeConfirmationTimer.Stop();

            SettingWindowView window = new();
            window.ShowDialog();

            _steamGuardTimer.Start();

            _autoTradeConfirmationTimer.Interval = TimeSpan.FromSeconds(Manifest.PeriodicCheckingInterval);
            if (Manifest.AutoConfirmMarketTransactions)
            {
                _autoTradeConfirmationTimer.Start();
            }
        });

        public ICommand ImportAccountsCommand => new AsyncRelayCommand(async o =>
        {
            OpenFileDialog fileDialog = new()
            {
                Multiselect = true
            };

            if (fileDialog.ShowDialog() == false) return;

            await ImportFiles(fileDialog.FileNames);
        });

        public ICommand RefreshAccountCommand => new AsyncRelayCommand(async o =>
        {
            await ManifestModelService.GetAccounts();
        });

        public ICommand DeleteAccountCommand => new AsyncRelayCommand(async o =>
        {
            if (SelectedAccount is null) return;

            if (CustomMessageBox.Show("are you sure you want to delete a account from drive?", "Delete account", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                await ManifestModelService.DeleteSteamGuardAccount(SelectedAccount);
            }
        });

        public ICommand LoginWindowShowCommand => new RelayCommand(o =>
        {
            if (SelectedAccount is null) return;

            ShowLoginWindow(LoginType.Refresh);
        });

        public ICommand ForceRefreshSession => new AsyncRelayCommand(async o =>
        {
            if (await RefreshAccountSession())
            {
                CustomMessageBox.Show("Your session has been refreshed.", "Session refresh", MessageBoxButton.OK, MessageBoxImage.Information, TextAlignment.Center);
                
                await ManifestModelService.SaveManifest();

                return;
            }

            CustomMessageBox.Show("Failed to refresh your session.\nTry using the \"Login again\" option.", "Session refresh", MessageBoxButton.OK, MessageBoxImage.Error);
        });

        public ICommand DeactivateAuthenticator => new AsyncRelayCommand(async o =>
        {
            if (SelectedAccount is null) return;

            int scheme;
            switch (MessageBox.Show("Would you like to remove Steam Guard completely?\nYes - Remove Steam Guard completely.\nNo - Switch back to Email authentication", "Remove Steam Guard", MessageBoxButton.YesNoCancel))
            {
                case MessageBoxResult.None:
                case MessageBoxResult.OK:
                case MessageBoxResult.Cancel:
                    MessageBox.Show("Steam Guard was not removed. No action was taken.");
                    return;
                case MessageBoxResult.Yes:
                    scheme = 2;
                    break;
                case MessageBoxResult.No:
                    scheme = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (CustomMessageBox.Show("Are you sure ?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            if (SelectedAccount.GenerateSteamGuardCode() is not { } confCode)
                return;

            InputWindowView confirmationWindow = new();
            var confirmationDataContext = (confirmationWindow.DataContext as InputWindowViewModel)!;
            confirmationDataContext.Text = $"Removing Steam Guard from {SelectedAccount.AccountName}. Enter this confirmation code: {confCode}";

            if (confirmationWindow.ShowDialog() == false)
                return;

            string enteredCode = confirmationDataContext.InputString.ToUpper();
            if (enteredCode != confCode)
            {
                CustomMessageBox.Show("Confirmation codes do not match. Steam Guard not removed.");
                return;
            }

            if (SelectedAccount.DeactivateAuthenticator(scheme))
            {
                CustomMessageBox.Show($"Steam Guard {(scheme == 2 ? "removed completely" : "switched to emails")}. maFile will be deleted after hitting okay. If you need to make a backup, now's the time.");
                await ManifestModelService.DeleteSteamGuardAccount(SelectedAccount);
                await ManifestModelService.GetAccounts();
            }
            else
                CustomMessageBox.Show("Steam Guard failed to deactivate.");
        });

        public ICommand CheckNewVersionCommand => new AsyncRelayCommand(async o =>
        {
            UpdateService.CheckForUpdateModel model;

            try
            {
                model = await UpdateService.CheckForUpdate("SteamDesktopAuthenticatorCore.exe");
                if (!model.NeedUpdate)
                {
                    CustomMessageBox.Show("You are using the latest version");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                CustomMessageBox.Show("Failed to check update");
                return;
            }

            if (CustomMessageBox.Show($"Would you like to download new version {model.NewVersion} ?", "Update service", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                return;

            try
            {
                if (await UpdateService.DownloadAndInstall(model) is not { } newFile)
                    throw new ArgumentNullException();

                var settings = Settings.GetSettings();
                settings.Updated = true;
                settings.SaveSettings();

                Application.Current.Shutdown(0);
                Process.Start(newFile);
            }
            catch
            {
                CustomMessageBox.Show("Failed to download update");
            }
        });

        public ICommand SetUpNewAccountCommand => new RelayCommand(o =>
        {
            LoginWindowView window = new();
            window.ShowDialog();

            RefreshAccountCommand.Execute(null);
        });

        public ICommand ConfirmationsShowCommand => new RelayCommand(o =>
        {
            if (SelectedAccount is null)
                return;

            if (Manifest.AutoConfirmMarketTransactions && Manifest.AutoConfirmTrades)
            {
                CustomMessageBox.Show("Disable auto confirm trades, or auto confirmation trades to confirm your trades manual");
                return;
            }

            _confirmationsWindow.Show();
        });

        public ICommand DragOverCommand => new RelayCommand(o =>
        {
            if (o is not DragEventArgs eventArgs)
                return;

            if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
                return;

            foreach (var file in files)
            {
                string extention = Path.GetExtension(file);

                if (Directory.Exists(file))
                {
                    eventArgs.Effects = DragDropEffects.None;
                    eventArgs.Handled = true;
                    return;
                }

                if (!extention.Contains(".exe")) continue;

                eventArgs.Effects = DragDropEffects.None;
                eventArgs.Handled = true;
                return;
            }

            eventArgs.Effects = DragDropEffects.Copy;
            eventArgs.Handled = true;
        });

        public ICommand DragAndDropCommand => new AsyncRelayCommand(async o =>
        {
            if (o is not DragEventArgs eventArgs)
                return;

            if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
                return;

            try
            {
                await ImportFiles(files);
            }
            catch
            {
                CustomMessageBox.Show("Something went wrong.\nMaybe you're not dragging a .maFile?");
            }
            finally
            {
                eventArgs.Handled = true;
            }
        });

        public ICommand OnFilterTextChangedCommand => new RelayCommand(o =>
        {
            
        });

        #endregion

        #region PrivateMethods

        private static async Task ImportFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                string fileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(file), ".maFile");

                await using FileStream stream = File.OpenRead(file);
                using StreamReader streamReader = new(stream);

                try
                {
                    await ManifestModelService.AddSteamGuardAccount(fileName, await streamReader.ReadToEndAsync());
                }
                catch
                {
                    CustomMessageBox.Show("Your file is corrupted!\nOr you didn't drag .maFile");
                }
            }

            await ManifestModelService.GetAccounts();
        }

        private void LoadAccountInfo()
        {
            if (SelectedAccount is null || _steamTime == 0) return;

            if (SelectedAccount.GenerateSteamGuardCodeForTime(_steamTime) is not { } token)
                return;

            LoginTokenText = token;
        }

        private async void SteamGuardTimerOnTick(object? sender, EventArgs e)
        {
            StatusText = "Aligning time with Steam...";
            _steamTime = await TimeAligner.GetSteamTimeAsync();
            StatusText = string.Empty;

            _currentSteamChunk = _steamTime / 30L;
            int secondsUntilChange = (int)(_steamTime - (_currentSteamChunk * 30L));

            LoadAccountInfo();
            if (SelectedAccount is not null)
            {
                ProgressBar = 30 - secondsUntilChange;
            }
        }

        private async Task AutoTradeConfirmationTimerOnTick()
        {
            Dictionary<SteamGuardAccount, List<ConfirmationModel>> autoAcceptConfirmations = new();

            SteamGuardAccount[] accounts = SelectedAccount is not null
                ? Manifest.CheckAllAccounts ? Manifest.Accounts.ToArray() : new[] {SelectedAccount}
                : Manifest.Accounts.ToArray();

            StatusText = "Checking confirmations...";

            foreach (var account in accounts)
            {
                try
                {
                    ConfirmationModel[] tmp = await account.FetchConfirmationsAsync();
                    foreach (var confirmationModel in tmp)
                    {
                        if (confirmationModel.ConfType == ConfirmationModel.ConfirmationType.MarketSellTransaction && Manifest.AutoConfirmMarketTransactions)
                        {
                            if (!autoAcceptConfirmations.ContainsKey(account))
                                autoAcceptConfirmations[account] = new List<ConfirmationModel>();

                            autoAcceptConfirmations[account].Add(confirmationModel);
                        }
                    }
                }
                catch (SteamGuardAccount.WgTokenInvalidException)
                {
                    StatusText = "Refreshing session";
                    await account.RefreshSessionAsync();
                    StatusText = "";
                }
                catch (SteamGuardAccount.WgTokenExpiredException)
                {
                    //Prompt to relogin
                    ShowLoginWindow(LoginType.Refresh);
                    break;

                }
                catch (WebException)
                {

                }
            }

            foreach (var account in autoAcceptConfirmations.Keys)
            {
                account.SendConfirmationAjax(autoAcceptConfirmations[account], SteamGuardAccount.Confirmation.Allow);
            }
        }

        private async Task<bool> RefreshAccountSession(bool attemptRefreshLogin = true)
        {
            if (SelectedAccount is null) return false;

            try
            {
                bool refreshed = await SelectedAccount.RefreshSessionAsync();
                return refreshed; //No exception thrown means that we either successfully refreshed the session or there was a different issue preventing us from doing so.
            }
            catch (SteamGuardAccount.WgTokenExpiredException)
            {
                if (!attemptRefreshLogin) return false;

                ShowLoginWindow(LoginType.Refresh);

                return await RefreshAccountSession(false);
            }
        }

        private void ShowLoginWindow(LoginType type)
        {
            LoginWindowView window = new();
            var dataContext = (window.DataContext as LoginWindowViewModel)!;
            dataContext.Account = SelectedAccount; //-V3149
            dataContext.LoginType = type;

            window.ShowDialog();
        }

        #endregion
    }
}