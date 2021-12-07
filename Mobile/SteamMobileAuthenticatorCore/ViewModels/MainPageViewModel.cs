using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamMobileAuthenticatorCore.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.ViewModels
{
    class MainPageViewModel : BaseViewModel, IDisposable
    {
        public MainPageViewModel()
        {
            _manifestModelService = App.ManifestModelService;
            Accounts = App.Accounts;

            _steamGuardTimer = new Timer(SteamGuardTimerOnTick);
            _steamGuardTimer.Start(TimeSpan.FromSeconds(2));
        }

        private readonly IManifestModelService _manifestModelService;

        #region Properties

        private SteamGuardAccount? _selectedSteamGuardAccount;
        private double _progressBar;
        private string _loginToken = string.Empty;
        private readonly Timer _steamGuardTimer;

        public ObservableCollection<SteamGuardAccount> Accounts { get; }

        public SteamGuardAccount? SelectedSteamGuardAccount
        {
            get => _selectedSteamGuardAccount;
            set => SetProperty(ref _selectedSteamGuardAccount, value);
        }

        public double ProgressBar
        {
            get => _progressBar;
            set => SetProperty(ref _progressBar, value);
        }

        public string LoginToken
        {
            get => _loginToken;
            set => SetProperty(ref _loginToken, value);
        }

        #endregion

        public ICommand OnLoadCommand => new AsyncCommand(async () =>
        {
            Accounts.Clear();

            foreach (var accounts in await _manifestModelService.GetAccounts())
                Accounts.Add(accounts);


            var manifest = _manifestModelService.GetManifestModel();
            if (manifest.AutoConfirmMarketTransactions)
                App.AutoMarketSetTimer.Start(TimeSpan.FromSeconds(manifest.PeriodicCheckingInterval));
        });

        public ICommand ImportCommand => new AsyncCommand(async () =>
        {
            IEnumerable<FileResult> files;

            try
            {
                files = await FilePicker.PickMultipleAsync(new PickOptions()
                {
                    PickerTitle = "Select maFile"
                }) ?? Array.Empty<FileResult>();
            }
            catch
            {
                files = Array.Empty<FileResult>();
            }

            foreach (var file in files)
            {
                try
                {
                    using var stream = await file.OpenReadAsync();
                    if (await _manifestModelService.AddSteamGuardAccount(stream, file.FileName) is { } account)
                        Accounts.Add(account);
                }
                catch
                {
                    //
                }
            }
        });

        public ICommand CopyCommand => new AsyncCommand(async () =>
        {
            await Clipboard.SetTextAsync(LoginToken);
        });

        private async Task SteamGuardTimerOnTick()
        {
            if (SelectedSteamGuardAccount is null)
                return;

            var steamTime = await TimeAligner.GetSteamTimeAsync();
            if (steamTime == 0)
                return;

            var currentSteamTime = steamTime / 30L;
            var secondsUntilChange = (int)(steamTime - currentSteamTime * 30L);

            Device.BeginInvokeOnMainThread(() =>
            {
                LoginToken = SelectedSteamGuardAccount.GenerateSteamGuardCodeForTime(steamTime) ?? string.Empty;
                ProgressBar = Convert.ToDouble(30 - secondsUntilChange) / 30;
            });
        }

        public void Dispose()
        {
            _steamGuardTimer.Stop();
        }
    }
}
