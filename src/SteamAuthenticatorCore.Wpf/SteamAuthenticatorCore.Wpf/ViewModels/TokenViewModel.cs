using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Helpers;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;
using Wpf.Ui.Controls.ContentDialogControl;
using Wpf.Ui.Controls.Navigation;
using Wpf.Ui.TaskBar;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class TokenViewModel : ObservableRecipient
{
    public TokenViewModel(ISteamGuardAccountService steamAccountService,
        AccountsServiceResolver accountsServiceResolver, ITimer timer)
    {
        _steamAccountService = steamAccountService;
        _timer = timer;

        _accountsService = accountsServiceResolver.Invoke();
        _appSettings = AppSettings.Current;
    }

    private readonly ISteamGuardAccountService _steamAccountService;
    private readonly ITimer _timer;
    private readonly IAccountsService _accountsService;
    private readonly AppSettings _appSettings;
    private StackPanel? _stackPanel;
    private Int64 _currentSteamChunk;

    [ObservableProperty]
    private IReadOnlyList<SteamGuardAccount> _accounts = Array.Empty<SteamGuardAccount>();

    [ObservableProperty]
    private string _token = string.Empty;

    [ObservableProperty]
    private double _tokenProgressBar;

    [ObservableProperty]
    private SteamGuardAccount? _selectedAccount;

    protected override void OnActivated()
    {
        base.OnActivated();

        _timer.StartOrRestart(TimeSpan.FromSeconds(1), OnTimer);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        _stackPanel = null;
    }

    [RelayCommand]
    private Task PageLoaded(StackPanel stackPanel)
    {
        _stackPanel = stackPanel;

        return RefreshAccounts();
    }

    [RelayCommand]
    private async Task RefreshAccounts()
    {
        Token = string.Empty;
        TokenProgressBar = 0;

        _stackPanel!.Visibility = Visibility.Visible;
        TaskBarService.Default.SetState(TaskBarProgressState.Indeterminate);

        try
        {
            await _accountsService.Initialize();
            Accounts = await _accountsService.GetAll();
        }
        finally
        {
            TaskBarService.Default.SetState(TaskBarProgressState.None);
            _stackPanel!.Visibility = Visibility.Hidden;
        }
    }

    [RelayCommand]
    private async Task ImportAccounts()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            CheckFileExists = true,
            Filter = $"mafile| *{IAccountsService.AccountFileExtension}"
        };

        if (fileDialog.ShowDialog() == false)
            return;

        await SaveAccountsFromFilesNames(fileDialog.FileNames);
    }

    [RelayCommand]
    private async Task ForceRefreshSession()
    {
        if (SelectedAccount is null)
            return;

        var dialog = ContentDialogService.Default.CreateDialog();
        dialog.Title = "Refresh session";

        if (!await _steamAccountService.RefreshSession(SelectedAccount, CancellationToken.None))
        {
            dialog.Content = "Failed to refresh session";
        }
        else
        {
            await _accountsService.Save(SelectedAccount);
            dialog.Content = "Session has been refreshed";   
        }

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async void ShowAccountFilesFolder()
    {
        if (_appSettings.AccountsLocation == AccountsLocation.GoogleDrive)
        {
            var dialog = ContentDialogService.Default.CreateDialog();
            dialog.Title = "Error";
            dialog.Content = $"Your accounts are stored in the google drive {App.InternalName} folder";

            await dialog.ShowAsync();
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SteamAuthenticatorCore.Desktop"),
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }
        catch (Exception)
        {
            //
        }
    }

    [RelayCommand]
    private async Task DeleteAccount()
    {
        if (SelectedAccount is null)
            return;

        var dialog = ContentDialogService.Default.CreateDialog();
        dialog.Title = "Delete account";
        dialog.Content = $"Are you sure what you want to delete {SelectedAccount.AccountName}?";
        dialog.PrimaryButtonText = "Yes";
        dialog.CloseButtonText = "No";

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        await _accountsService.Delete(SelectedAccount);
        SelectedAccount = null;

        await RefreshAccounts();
    }

    [RelayCommand]
    private void LoginAgain(SteamGuardAccount account)
    {
        NavigationService.Default.NavigateWithHierarchy(typeof(LoginPage));
        Messenger.Send(new UpdateAccountInLoginPageMessage(account));
    }

    [RelayCommand]
    private static void ListBoxDragOver(DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);

            if (Directory.Exists(file))
            {
                eventArgs.Effects = DragDropEffects.None;
                eventArgs.Handled = true;
                return;
            }

            if (extension.Contains(IAccountsService.AccountFileExtension))
                continue;

            eventArgs.Effects = DragDropEffects.None;
            eventArgs.Handled = true;
            return;
        }

        eventArgs.Effects = DragDropEffects.Copy;
        eventArgs.Handled = true;
    }

    [RelayCommand]
    private async Task ListBoxDragAndDrop(DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        await SaveAccountsFromFilesNames(files);
    }

    private async ValueTask SaveAccountsFromFilesNames(string[] files)
    {
        foreach (var fileName in files)
        {
            using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            await _accountsService.Save(stream, Path.GetFileName(fileName));
        }

        await RefreshAccounts();
    }

    private void OnTimer(CancellationToken obj)
    {
        if (SelectedAccount is null)
            return;

        var steamTime = ITimeAligner.SteamTime;
        _currentSteamChunk = steamTime / 30L;
        var secondsUntilChange = (int)(steamTime - _currentSteamChunk * 30L);

        if (SelectedAccount.GenerateSteamGuardCode() is { } token)
            Token = token;

        TokenProgressBar = 30 - secondsUntilChange;
    }
}