using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using SteamAuthenticatorCore.Shared.ViewModel;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class TokenViewModel : TokenViewModelBase, IDisposable
{
    public TokenViewModel(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, IPlatformImplementations platformImplementations, ManifestAccountsWatcherService accountsWatcherService, TaskBarServiceWrapper taskBarServiceWrapper, IDialogService dialogService, INavigationService navigationService, AppSettings appSettings, IMessenger messenger) : base(accounts, platformTimer, platformImplementations)
    {
        _platformImplementations = platformImplementations;
        _accountsWatcherService = accountsWatcherService;
        _taskBarServiceWrapper = taskBarServiceWrapper;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _appSettings = appSettings;
        _messenger = messenger;
    }

    private readonly IPlatformImplementations _platformImplementations;
    private readonly ManifestAccountsWatcherService _accountsWatcherService;
    private readonly TaskBarServiceWrapper _taskBarServiceWrapper;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly AppSettings _appSettings;
    private readonly IMessenger _messenger;

    private StackPanel? _stackPanel;

    public void Dispose()
    {
        _appSettings.PropertyChanged -= AppSettingsOnPropertyChanged;
        _stackPanel = null;
    }

    [RelayCommand]
    private async void PageLoaded(StackPanel stackPanel)
    {
        if (_stackPanel is not null)
            return;

        _stackPanel = stackPanel;

        await PerformLoadingOperation(async () =>
        {
            await _accountsWatcherService.Initialize();
        });

        _appSettings.PropertyChanged += AppSettingsOnPropertyChanged;
    }

    [RelayCommand]
    private Task ImportAccounts()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            CheckFileExists = true,
            Filter = $"mafile| *{ManifestModelServiceConstants.FileExtension}"
        };

        if (fileDialog.ShowDialog() == false)
            return Task.CompletedTask;

        return _accountsWatcherService.ImportSteamGuardAccounts(fileDialog.FileNames);
    }

    [RelayCommand]
    private async Task RefreshAccounts(StackPanel stackPanel)
    {
        await PerformLoadingOperation(async () =>
        {
            await _accountsWatcherService.RefreshAccounts();
        });
    }

    [RelayCommand]
    private async Task ShowAccountFilesFolder()
    {
        if (_appSettings.ManifestLocation == ManifestLocationModel.GoogleDrive)
        {

            var control = _dialogService.GetDialogControl();

            await control.ShowAndWaitAsync("Alert!",
                $"Your accounts are stored in the google drive {App.InternalName} folder");

            control.Hide();
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

        var control = _dialogService.GetDialogControl();
        var res = await control.ShowAndWaitAsync("Alser", $"Are you sure you want to delete {SelectedAccount.AccountName}?");
        if (res != IDialogControl.ButtonPressed.Left)
        {
            control.Hide();
            return;
        }

        control.Hide();
        await _accountsWatcherService.DeleteAccount(SelectedAccount);
    }

    [RelayCommand]
    private void LoginAgain(SteamGuardAccount account)
    {
        _navigationService.NavigateTo("/login");
        _messenger.Send(new UpdateAccountInLoginPageMessage(account));
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

            if (extension.Contains(ManifestModelServiceConstants.FileExtension)) continue;

            eventArgs.Effects = DragDropEffects.None;
            eventArgs.Handled = true;
            return;
        }

        eventArgs.Effects = DragDropEffects.Copy;
        eventArgs.Handled = true;
    }

    [RelayCommand]
    private Task ListBoxDragAndDrop(DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            return Task.CompletedTask;

        if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return Task.CompletedTask;

        return _accountsWatcherService.ImportSteamGuardAccounts(files);
    }

    private async void AppSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await PerformLoadingOperation(async () =>
        {
            await _accountsWatcherService.Initialize();
        });
    }

    private async ValueTask PerformLoadingOperation(Func<ValueTask> func)
    {
        Token = string.Empty;
        TokenProgressBar = 0;

        _stackPanel!.Visibility = Visibility.Visible;
        _taskBarServiceWrapper.SetState(TaskBarProgressState.Indeterminate);

        try
        {
            await func.Invoke();
        }
        finally
        {
            _taskBarServiceWrapper.SetState(TaskBarProgressState.None);
            _stackPanel!.Visibility = Visibility.Hidden;
        }
    }
}