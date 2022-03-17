using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using WPFUI.Common;
using WPFUI.DIControls;
using WPFUI.Taskbar;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class TokenViewModel : ObservableObject
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

    private bool _loaded;
    private UIElement _loadingElement = null!;

    #endregion

    #region Properties

    public ObservableCollection<SteamGuardAccount> Accounts { get; }

    public SteamGuardAccount? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            SetProperty(ref _selectedAccount, value);
            TokenService.SelectedAccount = value;
        }
    }

    public TokenService TokenService { get; }

    #endregion

    #region Commands

    [ICommand]
    private void WindowLoaded(UIElement loadingElement)
    {
        if (_loaded) return;

        _loaded = true;
        _loadingElement = loadingElement;
    }

    [ICommand]
    private async Task DeleteAccount(object o)
    {
        if (SelectedAccount is null)
            return;

        if (await _dialog.ShowDialog($"Are you sure you want to delete {SelectedAccount.AccountName}?", App.Name, "Yes", "No") != ButtonPressed.Left)
            return;

        await _manifestServiceResolver.Invoke().DeleteSteamGuardAccount(SelectedAccount);
        Accounts.Remove(SelectedAccount);
    }

    [ICommand]
    private void ListBoxDragOver(object o)
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
    }

    [ICommand]
    private async Task ListBoxDragAndDrop(object o)
    {
        DragEventArgs eventArgs = (o as DragEventArgs)!;

        if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        await ImportSteamGuardAccount(files);
    }

    [ICommand]
    private async Task ImportAccounts()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            CheckFileExists = true,
            Filter = $"mafile| *{ManifestModelServiceConstants.FileExtension}"
        };

        if (fileDialog.ShowDialog() == false) return;

        await ImportSteamGuardAccount(fileDialog.FileNames);
    }

    [ICommand]
    private async Task RefreshAccounts()
    {
        TokenService.Token = string.Empty;
        TokenService.TokenProgressBar = 0;
        _loadingElement.Visibility = Visibility.Visible;
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
            _loadingElement.Visibility = Visibility.Hidden;
            Progress.SetState(ProgressState.None);
        }
    }

    [ICommand]
    private async Task ShowAccountFilesFolder()
    {
        if (_appSettings.ManifestLocation == AppSettings.ManifestLocationModel.GoogleDrive)
        {
            await _dialog.ShowDialog("", $"Your accounts are stored in the google drive {App.InternalName} folder");
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
    }

    [ICommand]
    private void LoginInSelectedAccount()
    {
        if (SelectedAccount == null)
            return;

        _navigation.NavigateTo($"//{nameof(LoginPage)}", new object[] {SelectedAccount});
    }

    [ICommand]
    private async Task ForceRefreshSession()
    {
        if (SelectedAccount is null)
            return;

        if (await RefreshAccountSession(SelectedAccount))
        {
            await _dialog.ShowDialog("Your session has been refreshed.", "Session refresh");
            return;
        }

        await _dialog.ShowDialog("Failed to refresh your session.\nTry using the \"Login again\" option.", "Session refresh");
    }

    #endregion

    #region PublicMethods

    public async Task OnManifestLocationChanged()
    {
        var manifestService = _manifestServiceResolver.Invoke();
        await manifestService.Initialize();

        await RefreshAccounts();
    }

    #endregion

    #region PrivateMethods

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