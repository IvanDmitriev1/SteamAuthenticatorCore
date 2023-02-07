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
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.ViewModel;
using Wpf.Ui.Contracts;
using Wpf.Ui.TaskBar;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class TokenViewModel : TokenViewModelBase, IDisposable
{
    public TokenViewModel(ObservableCollection<SteamGuardAccount> accounts, IValueTaskTimer valueTaskTimer,
        IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService,
        AccountsFileServiceResolver accountsFileServiceResolver, AppSettings appSettings,
        INavigationService navigationService, IMessenger messenger, TaskBarServiceWrapper taskBarServiceWrapper) : base(
        accounts, valueTaskTimer, platformImplementations, accountService, accountsFileServiceResolver)
    {
        _appSettings = appSettings;
        _navigationService = navigationService;
        _messenger = messenger;
        _taskBarServiceWrapper = taskBarServiceWrapper;
    }

    private readonly AppSettings _appSettings;
    private readonly INavigationService _navigationService;
    private readonly IMessenger _messenger;
    private readonly TaskBarServiceWrapper _taskBarServiceWrapper;
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

        await RefreshAccounts();

        _appSettings.PropertyChanged += AppSettingsOnPropertyChanged;
    }

    [RelayCommand]
    private async Task ImportAccounts()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            CheckFileExists = true,
            Filter = $"mafile| *{IAccountsFileService.AccountFileExtension}"
        };

        if (fileDialog.ShowDialog() == false)
            return;

        var accountsService = AccountsFileServiceResolver.Invoke();

        foreach (var filePath in fileDialog.FileNames)
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await accountsService.SaveAccount(stream, Path.GetFileName(filePath));
        }
    }

    [RelayCommand]
    private Task ForceRefreshSession()
    {
        if (SelectedAccount is null)
            return Task.CompletedTask;

        return RefreshAccountsSession(SelectedAccount);
    }

    [RelayCommand]
    private async Task RefreshAccounts()
    {
        Token = string.Empty;
        TokenProgressBar = 0;

        _stackPanel!.Visibility = Visibility.Visible;
        _taskBarServiceWrapper.SetState(TaskBarProgressState.Indeterminate);

        try
        {
            await AccountsFileServiceResolver.Invoke().InitializeOrRefreshAccounts();
        }
        finally
        {
            _taskBarServiceWrapper.SetState(TaskBarProgressState.None);
            _stackPanel!.Visibility = Visibility.Hidden;
        }
    }

    [RelayCommand]
    private async Task ShowAccountFilesFolder()
    {
        if (_appSettings.AccountsLocation == AccountsLocationModel.GoogleDrive)
        {
            await PlatformImplementations.DisplayAlert("Error",
                $"Your accounts are stored in the google drive {App.InternalName} folder");
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

        await DeleteAccount(SelectedAccount);
    }

    [RelayCommand]
    private void LoginAgain(SteamGuardAccount account)
    {
        //TODO

        /*_navigationService.NavigateTo("/login");
        _messenger.Send(new UpdateAccountInLoginPageMessage(account));*/
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

            if (extension.Contains(IAccountsFileService.AccountFileExtension)) continue;

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

        var accountsService = AccountsFileServiceResolver.Invoke();

        foreach (var fileName in files)
        {
            await using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            await accountsService.SaveAccount(stream, Path.GetFileName(fileName));
        }
    }

    private async void AppSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppSettings.AccountsLocation))
            return;

        await RefreshAccounts();
    }
}