using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Helpers;
using SteamAuthenticatorCore.Mobile.Services;
using SteamAuthenticatorCore.Mobile.Views;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels
{
    class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            _manifestModelService = DependencyService.Get<IManifestModelService>();
            Accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();

            var steamGuardTimer = new Timer(SteamGuardTimerOnTick);
            steamGuardTimer.Start(TimeSpan.FromSeconds(2));
        }

        private readonly IManifestModelService _manifestModelService;

        #region Properties

        private SteamGuardAccount? _selectedSteamGuardAccount;
        private bool _loaded;
        private double _progressBar;
        private string _loginToken = string.Empty;

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
            if (_loaded)
                return;

            _loaded = true;
            Accounts.Clear();

            await _manifestModelService.Initialize(new MobileDirectoryService());

            foreach (var accounts in await _manifestModelService.GetAccounts())
                Accounts.Add(accounts);

            var manifest = _manifestModelService.GetManifestModel();
            if (manifest.AutoConfirmMarketTransactions)
            {
                //App.AutoMarketSellTimer.Start(TimeSpan.FromSeconds(manifest.PeriodicCheckingInterval));
            }
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
                    await Application.Current.MainPage.DisplayAlert("Import account", $"failed to import {file.FileName}, maybe your file is corrupted?", "Ok");
                }
            }
        });

        public ICommand CopyCommand => new AsyncCommand(async () =>
        {
            await Clipboard.SetTextAsync(LoginToken);
        });

        public ICommand DeleteCommand => new AsyncCommand<SteamGuardAccount>(async o =>
        {
            if (await Application.Current.MainPage.DisplayAlert("Delete account", "are you sure?", "yes", "no"))
            {
                await _manifestModelService.DeleteSteamGuardAccount(o!);
                Accounts.Remove(o!);
            }
        });

        public ICommand OnLoginCommand => new AsyncCommand<SteamGuardAccount>(async (o) =>
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}?id={Accounts.IndexOf(o!)}");
        });

        public ICommand ForceRefreshSession => new AsyncCommand<SteamGuardAccount>(async (o) =>
        {
            if (await o!.RefreshSessionAsync())
                await Application.Current.MainPage.DisplayAlert("Refresh session", "the session has been refreshed", "Ok");
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
                LoginToken = SelectedSteamGuardAccount.GenerateSteamGuardCode(steamTime) ?? string.Empty;
                ProgressBar = Convert.ToDouble(30 - secondsUntilChange) / 30;
            });
        }
    }
}
