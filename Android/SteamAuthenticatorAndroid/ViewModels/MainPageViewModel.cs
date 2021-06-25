using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Models;
using SteamAuthenticatorAndroid.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorAndroid.ViewModels
{
    internal class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            Task.Run(async () =>
            {
                Manifest = await ManifestModelService.GetManifest();

                AutoConfirmTradesTimer = new Timer(Manifest.PeriodicCheckingInterval, AutoTradeConfirmationTimerOnTick);

                if (Manifest!.AutoConfirmTrades)
                {
                    AutoConfirmTradesTimer.Start();
                }
            });

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                SteamGuardTimerOnTick();

                return true;
            });

            ImportAccount = new Command(AddMaFile);
            DeleteAccount = new Command(DeleteAccountMethod);
            CopyCommand = new Command(CopyCommandMethod);
            MoveAccountUpCommand = new Command(MoveAccountUp);
            MoveAccountDownCommand = new Command(MoveAccountDown);
            LoginCommand = new Command(ShowLoginWindow);

            SettingsPageViewModel.Page = this;
        }

        #region Variables

        private long _steamTime;
        private long _currentSteamChunk;

        private ManifestModel? _manifest;
        private SteamGuardAccount? _selectedAccount;
        private string _loginTokenText = string.Empty;
        private string _statusText = string.Empty;
        private double _progressBar;

        #endregion

        #region Fields

        public Timer AutoConfirmTradesTimer = null!;

        public static SteamGuardAccount? SelectedGuardAccount;

        public ManifestModel? Manifest
        {
            get => _manifest;
            private set => SetProperty(ref _manifest, value);
        }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                SelectedGuardAccount = value;
                SetProperty(ref _selectedAccount, value);
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string LoginTokenText
        {
            get => _loginTokenText;
            set => SetProperty(ref _loginTokenText, value);
        }

        public double ProgressBar
        {
            get => _progressBar;
            set
            {
                value /= 30;

                SetProperty(ref _progressBar, value);
            }
        }

        #endregion

        #region Commands

        public Command ImportAccount { get; }

        public Command CopyCommand { get; }

        public Command DeleteAccount { get; }

        public Command MoveAccountUpCommand { get; }

        public Command MoveAccountDownCommand { get; }

        public Command LoginCommand { get; }

        #endregion

        #region Methods

        private void LoadAccountInfo()
        {
            if (SelectedAccount is null || _steamTime == 0) return;

            if (SelectedAccount.GenerateSteamGuardCodeForTime(_steamTime) is not { } token)
                return;

            LoginTokenText = token;
        }

        private async void SteamGuardTimerOnTick()
        {
            StatusText = "Aligning time with Steam...";
            _steamTime = await TimeAligner.GetSteamTimeAsync();
            StatusText = string.Empty;

            _currentSteamChunk = _steamTime / 30L;
            var secondsUntilChange = (int) (_steamTime - _currentSteamChunk * 30L);

            LoadAccountInfo();
            if (SelectedAccount is not null)
                ProgressBar = 30 - secondsUntilChange;
        }

        private static async void AddMaFile()
        {
            FileResult[] files;

            try
            {
                files = (await FilePicker.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = "Select maFile"
                })).ToArray();
            }
            catch
            {
                return;
            }

            foreach (var file in files)
            {
                if (!Path.GetExtension(file.FileName).Contains("maFile")) continue;

                await ManifestModelService.AddSteamGuardAccount(file.FullPath);
            }
        }

        private async void DeleteAccountMethod()
        {
            if (SelectedAccount is null) return;

            await ManifestModelService.DeleteSteamGuardAccount(SelectedAccount);
        }

        private void CopyCommandMethod()
        {
            Clipboard.SetTextAsync(LoginTokenText);
        }

        private async void MoveAccountUp(object obj)
        {
            if (Manifest is not { } ) return;
            if(obj is not SteamGuardAccount account) return;


            int index = Manifest.Accounts.IndexOf(account);
            if (index < 0 && Manifest.Accounts.Count <= 0) return;

            Manifest.Accounts.Move(index -1, index);
            await ManifestModelService.SaveManifest();
        }

        private async void MoveAccountDown(object obj)
        {
            if (Manifest is not { }) return;
            if (obj is not SteamGuardAccount account) return;

            int index = Manifest.Accounts.IndexOf(account);
            if (index + 1 >= Manifest.Accounts.Count && Manifest.Accounts.Count <= 0) return;

            Manifest.Accounts.Move(index, index +1);
            await ManifestModelService.SaveManifest();
        }

        private async void AutoTradeConfirmationTimerOnTick()
        {
            List<ConfirmationModel> confs = new();
            Dictionary<SteamGuardAccount, List<ConfirmationModel>> autoAcceptConfirmations = new();
            SteamGuardAccount[] accs = Manifest!.Accounts.ToArray();

            StatusText = "Checking confirmations...";

            foreach (var acc in accs)
            {
                try
                {
                    ConfirmationModel[] tmp = await acc.FetchConfirmationsAsync();
                    foreach (var conf in tmp)
                    {
                        if ((conf.ConfType == ConfirmationModel.ConfirmationType.MarketSellTransaction && Manifest.AutoConfirmMarketTransactions) ||
                            (conf.ConfType == ConfirmationModel.ConfirmationType.Trade && Manifest.AutoConfirmTrades))
                        {
                            if (!autoAcceptConfirmations.ContainsKey(acc))
                                autoAcceptConfirmations[acc] = new List<ConfirmationModel>();
                            autoAcceptConfirmations[acc].Add(conf);
                        }
                        else
                            confs.Add(conf);
                    }
                }
                catch (SteamGuardAccount.WgTokenInvalidException)
                {
                    StatusText = "Refreshing session";
                    await acc.RefreshSessionAsync(); //Don't save it to the HDD, of course. We'd need their encryption passkey again.
                    StatusText = "";
                }
                catch (SteamGuardAccount.WgTokenExpiredException)
                {
                    //Prompt to relogin
                    //ShowLoginWindow(LoginType.Refresh);
                    break; //Don't bombard a user with login refresh requests if they have multiple accounts. Give them a few seconds to disable the autocheck option if they want.
                }
                catch (WebException)
                {

                }
            }

            foreach (var acc in autoAcceptConfirmations.Keys)
            {
                var confirmations = autoAcceptConfirmations[acc].ToArray();
                acc.AcceptMultipleConfirmations(confirmations);
            }
        }

        private void ShowLoginWindow()
        {
            
        }

        #endregion
    }
}