﻿using System.Reflection;
using System.Threading;
using Wpf.Ui.Controls;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class SettingsViewModel : MyObservableRecipient
{
    public SettingsViewModel(IUpdateService updateService, ILogger<SettingsViewModel> logger, AccountsServiceResolver accountsServiceResolver)
    {
        _updateService = updateService;
        _logger = logger;
        _accountsServiceResolver = accountsServiceResolver;
        AppSettings = AppSettings.Current;
        CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
        IsMaFilesLocationСhangeable = GoogleDriveAccountsService.IsClientSecretAttachedToAssembly();
    }

    private readonly IUpdateService _updateService;
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly AccountsServiceResolver _accountsServiceResolver;

    public AppSettings AppSettings { get; }
    public string CurrentVersion { get; }

    [ObservableProperty]
    private bool _isMaFilesLocationСhangeable;

    protected override void OnActivated()
    {
        base.OnActivated();

        AppSettings.PropertyChanged += AppSettingsOnPropertyChanged;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        AppSettings.PropertyChanged += AppSettingsOnPropertyChanged;
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        try
        {
            if (await _updateService.CheckForUpdate() is not { } release)
            {
                SnackbarService.Default.Show(AppSettings.LocalizationProvider[LocalizationMessage.UpdaterMessage],
                    AppSettings.LocalizationProvider[LocalizationMessage.YouAreUsingTheLatestVersionMessage],
                    ControlAppearance.Secondary, new SymbolIcon(SymbolRegular.Info24)
                    {
                        FontSize = 32
                    });
                return;
            }

            var contentDialog = new DownloadUpdateContentDialog(ContentDialogService.Default.GetContentPresenter(), release);
            await contentDialog.ShowAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"{nameof(CheckForUpdates)} method");
        }
    }

    private async void AppSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppSettings.AccountsLocation))
            return;

        var cts = new CancellationTokenSource();
        ContentDialog dialog = new ContentDialog(ContentDialogService.Default.GetContentPresenter())
        {
            Title = "Settings",
            Content = "Please wait",
            IsFooterVisible = false
        };

        try
        {
            var initializeTask = _accountsServiceResolver.Invoke().Initialize();
            var continueWithTask = initializeTask.ContinueWith(task => cts.Cancel(), cts.Token);

            await dialog.ShowAsync(cts.Token);
        }
        catch (TaskCanceledException)
        {
            dialog.Hide();
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Exception on property changed");
        }
    }
}
