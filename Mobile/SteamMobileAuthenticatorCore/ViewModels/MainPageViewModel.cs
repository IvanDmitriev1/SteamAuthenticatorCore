using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Views;
using SteamAuthenticatorCore.Shared;
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
            TokenService = DependencyService.Get<TokenService>();
        }

        private readonly IManifestModelService _manifestModelService;

        #region Properties

        private SteamGuardAccount? _selectedSteamGuardAccount;

        public ObservableCollection<SteamGuardAccount> Accounts { get; }

        public SteamGuardAccount? SelectedSteamGuardAccount
        {
            get => _selectedSteamGuardAccount;
            set
            {
                SetProperty(ref _selectedSteamGuardAccount, value);
                TokenService.SelectedAccount = value;
            }
        }

        public TokenService TokenService { get; }

        #endregion

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
            await Clipboard.SetTextAsync(TokenService.Token);
        });

        public ICommand DeleteCommand => new AsyncCommand<SteamGuardAccount>(async o =>
        {
            if (await Application.Current.MainPage.DisplayAlert("Delete account", "are you sure?", "yes", "no"))
            {
                await _manifestModelService.DeleteSteamGuardAccount(o!);
                Accounts.Remove(o!);
            }
        });

        public ICommand OnConfirmationsCommand => new AsyncCommand<SteamGuardAccount>(async (o) =>
        {
            SelectedSteamGuardAccount = Accounts[Accounts.IndexOf(o!)];
            await Shell.Current.GoToAsync($"{nameof(ConfirmationsPage)}?id={Accounts.IndexOf(o!)}");
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
    }
}
