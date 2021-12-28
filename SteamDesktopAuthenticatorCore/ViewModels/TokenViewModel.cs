using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamDesktopAuthenticatorCore.Common;
using WpfHelper.Commands;
using WpfHelper.Common;
using WpfHelper.Services;
using WPFUI.Common;
using WPFUI.Controls;
using MessageBox = WPFUI.Controls.MessageBox;
using RelayCommand = WpfHelper.Commands.RelayCommand;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class TokenViewModel : BaseViewModel
    {
        public TokenViewModel(App.ManifestServiceResolver manifestServiceResolver, SettingService settingsService)
        {
            _appSettings = settingsService.Get<AppSettings>();
            _manifestModelService = manifestServiceResolver.Invoke();
            Accounts = new ObservableCollection<SteamGuardAccount>();

            _steamGuardTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _steamGuardTimer.Tick += async (sender, args) => await SteamGuardTimerOnTick();

            _autoTradeConfirmationTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(_appSettings.PeriodicCheckingInterval)
            };
            _autoTradeConfirmationTimer.Tick += async (sender, args) => await AutoTradeConfirmationTimerOnTick();
        }

        public ObservableCollection<SteamGuardAccount> Accounts { get; }

        #region Variabls

        private readonly IManifestModelService _manifestModelService;
        private readonly DispatcherTimer _steamGuardTimer;
        private readonly DispatcherTimer _autoTradeConfirmationTimer;
        private readonly AppSettings _appSettings;

        private Int64 _currentSteamChunk;
        private Int64 _steamTime;

        private SteamGuardAccount? _selectedAccount;
        private string _token = "Login token";
        private int _tokenProgressBar;

        #endregion

        #region Fields

        public string Token
        {
            get => _token;
            set => Set(ref _token, value);
        }

        public int TokenProgressBar
        {
            get => _tokenProgressBar;
            set => Set(ref _tokenProgressBar, value);
        }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set => Set(ref _selectedAccount, value);
        }

        #endregion

        #region Commands

        public ICommand WindowLoadedCommand => new AsyncRelayCommand(async o =>
        {
            _steamGuardTimer.Start();

            if (_appSettings.AutoConfirmMarketTransactions)
                _autoTradeConfirmationTimer.Start();



            await RefreshAccounts();
        });

        public ICommand DeleteAccountCommand => new AsyncRelayCommand(async o =>
        {
            if (SelectedAccount is null)
                return;

            var dialog = Dialog.GetCurrentInstance();
            if (await dialog.ShowDialog("Are you sure", App.Name, "Yes", "No") != ButtonPressed.Left)
                return;

            await _manifestModelService.DeleteSteamGuardAccount(SelectedAccount);
            Accounts.Remove(SelectedAccount);
        });

        public ICommand ListBoxDragOverCommand => new RelayCommand(o =>
        {
            DragEventArgs eventArgs = (o as DragEventArgs)!;

            if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
                return;

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file);

                if (Directory.Exists(file))
                {
                    eventArgs.Effects = DragDropEffects.None;
                    eventArgs.Handled = true;
                    return;
                }

                if (extension.Contains(ManifestModelServiceConstants.FileExtension)) continue;

                eventArgs.Effects = DragDropEffects.None;
                eventArgs.Handled = true;
                return;
            }

            eventArgs.Effects = DragDropEffects.Copy;
            eventArgs.Handled = true;
        });

        public ICommand ListBoxDragAndDropCommand => new AsyncRelayCommand(async o =>
        {
            DragEventArgs eventArgs = (o as DragEventArgs)!;

            if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
                return;

            await ImportSteamGuardAccount(files);
        });

        #endregion

        #region PrivateMethods

        private async Task SteamGuardTimerOnTick()
        {
            _steamTime = await TimeAligner.GetSteamTimeAsync();
            _currentSteamChunk = _steamTime / 30L;
            int secondsUntilChange = (int)(_steamTime - (_currentSteamChunk * 30L));

            SetAccountToken();
            if (SelectedAccount is not null)
                TokenProgressBar = 30 - secondsUntilChange;
        }

        private async Task AutoTradeConfirmationTimerOnTick()
        {

        }

        private void SetAccountToken()
        {
            if (SelectedAccount is null || _steamTime == 0) return;

            if (SelectedAccount.GenerateSteamGuardCodeForTime(_steamTime) is not { } token)
                return;

            Token = token;
        }

        private async Task RefreshAccounts()
        {
            Accounts.Clear();

            try
            {
                foreach (var account in await _manifestModelService.GetAccounts())
                    Accounts.Add(account);
            }
            catch (Exception)
            {
                MessageBox box = new MessageBox()
                {
                    LeftButtonName = "Ok",
                    RightButtonName = "Cancel"
                };
                box.Show(App.Name, "One of your files is corrupted");
            }
        }

        private async Task ImportSteamGuardAccount(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                await using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);

                try
                {
                    if (await _manifestModelService.AddSteamGuardAccount(stream, stream.Name) is { } account)
                        Accounts.Add(account);
                }
                catch
                {
                    await Dialog.GetCurrentInstance().ShowDialog("Your file is corrupted!");
                }
            }
        }

        #endregion
    }
}
