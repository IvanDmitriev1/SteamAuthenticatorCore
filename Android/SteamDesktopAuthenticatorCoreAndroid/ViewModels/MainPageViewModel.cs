using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCoreAndroid.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamDesktopAuthenticatorCoreAndroid.ViewModels
{
    internal class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            Task.Run(async () =>
            {
                Manifest = await ManifestModelService.GetManifest();
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

        public ManifestModel? Manifest
        {
            get => _manifest;
            private set => SetProperty(ref _manifest, value);
        }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
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

        public void CopyCommandMethod()
        {
            Clipboard.SetTextAsync(LoginTokenText);
        }

        public async void MoveAccountUp(object obj)
        {
            if (Manifest is not { } ) return;
            if(obj is not SteamGuardAccount account) return;


            int index = Manifest.Accounts.IndexOf(account);
            if (index < 0 && Manifest.Accounts.Count <= 0) return;

            Manifest.Accounts.Move(index -1, index);
            await ManifestModelService.SaveManifest();
        }

        public async void MoveAccountDown(object obj)
        {
            if (Manifest is not { }) return;
            if (obj is not SteamGuardAccount account) return;

            int index = Manifest.Accounts.IndexOf(account);
            if (index + 1 >= Manifest.Accounts.Count && Manifest.Accounts.Count <= 0) return;

            Manifest.Accounts.Move(index, index +1);
            await ManifestModelService.SaveManifest();
        }

        #endregion
    }
}