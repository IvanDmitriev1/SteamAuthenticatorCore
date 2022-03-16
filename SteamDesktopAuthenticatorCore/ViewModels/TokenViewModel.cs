using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using WpfHelper.Commands;
using WPFUI.Common;
using WPFUI.DIControls;
using WPFUI.Taskbar;
using RelayCommand = WpfHelper.Commands.RelayCommand;

namespace SteamAuthenticatorCore.Desktop.ViewModels
{
    public class TokenViewModel : BaseViewModel
    {
        public TokenViewModel(AppSettings appSettings, App.ManifestServiceResolver manifestServiceResolver, TokenService tokenService, DefaultNavigation navigation, Dialog dialog, ObservableCollection<SteamGuardAccount> steamGuardAccounts)
        {
            _appSettings = appSettings;
            _manifestServiceResolver = manifestServiceResolver;
            TokenService = tokenService;
            _navigation = navigation;
            _dialog = dialog;
            Accounts = steamGuardAccounts;
        }

        #region Variabls

        private readonly AppSettings _appSettings;
        private readonly App.ManifestServiceResolver _manifestServiceResolver;
        private readonly DefaultNavigation _navigation;
        private readonly Dialog _dialog;

        private SteamGuardAccount? _selectedAccount;

        #endregion

        #region Properties

        public ObservableCollection<SteamGuardAccount> Accounts { get; }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                Set(ref _selectedAccount, value);
                TokenService.SelectedAccount = value;
            }
        }

        public TokenService TokenService { get; }

        #endregion

        #region Commands

        public ICommand WindowLoadedCommand => new RelayCommand( o =>
        {
            
        });

        public ICommand DeleteAccountCommand => new AsyncRelayCommand(async o =>
        {
            if (SelectedAccount is null)
                return;

            if (await _dialog.ShowDialog($"Are you sure you want to delete {SelectedAccount.AccountName}?", App.Name, "Yes", "No") != ButtonPressed.Left)
                return;

            await _manifestServiceResolver.Invoke().DeleteSteamGuardAccount(SelectedAccount);
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

        public ICommand ImportAccountsCommand => new AsyncRelayCommand(async o =>
        {
            OpenFileDialog fileDialog = new()
            {
                Multiselect = true,
                CheckFileExists = true,
                Filter = $"mafile| *{ManifestModelServiceConstants.FileExtension}"
            };

            if (fileDialog.ShowDialog() == false) return;

            await ImportSteamGuardAccount(fileDialog.FileNames);
        });

        public ICommand RefreshAccountsListCommand => new AsyncRelayCommand(async o =>
        {
            await RefreshAccounts();
        });

        public ICommand ShowAccountFilesFolder => new RelayCommand(o =>
        {
            if (_appSettings.ManifestLocation == AppSettings.ManifestLocationModel.GoogleDrive)
            {
                _dialog.ShowDialog("", $"Your accounts are stored in the google drive {App.InternalName} folder");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    Arguments = Path.Combine(Directory.GetCurrentDirectory(), "maFiles"),
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        });

        public ICommand LoginInSelectedAccountCommand => new RelayCommand(o =>
        {
            if (SelectedAccount == null)
                return;

            _navigation.NavigateTo($"//{nameof(LoginPage)}", new object[] {SelectedAccount});
        });

        public ICommand ForceRefreshSessionCommand => new AsyncRelayCommand(async o =>
        {
            if (SelectedAccount is null)
                return;

            if (await RefreshAccountSession(SelectedAccount))
            {
                await _dialog.ShowDialog("Your session has been refreshed.", "Session refresh");
                return;
            }

            await _dialog.ShowDialog("Failed to refresh your session.\nTry using the \"Login again\" option.", "Session refresh");
        });

        #endregion

        #region PrivateMethods

        private async Task RefreshAccounts()
        {
            Progress.SetState(ProgressState.Indeterminate);
            Accounts.Clear();

            try
            {
                foreach (var account in await _manifestServiceResolver.Invoke().GetAccounts())
                    Accounts.Add(account);
            }
            catch (Exception)
            {
                /*MessageBox box = new MessageBox()
                {
                    LeftButtonName = "Ok",
                    RightButtonName = "Cancel"
                };
                box.Show(App.Name, "One of your files is corrupted");*/
            }
            finally
            {
                Progress.SetState(ProgressState.None);
            }
        }

        private async Task ImportSteamGuardAccount(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                await using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);

                try
                {
                    if (await _manifestServiceResolver.Invoke().AddSteamGuardAccount(stream, stream.Name) is { } account)
                        Accounts.Add(account);
                }
                catch
                {
                    await _dialog.ShowDialog("", "Your file is corrupted!");
                }
            }
        }

        private async Task<bool> RefreshAccountSession(SteamGuardAccount account)
        {
            try
            {
                return await account.RefreshSessionAsync();
            }
            catch (SteamGuardAccount.WgTokenExpiredException)
            {
                _navigation.NavigateTo($"//{nameof(LoginPage)}");

                return await RefreshAccountSession(account);
            }
        }

        #endregion
    }
}
