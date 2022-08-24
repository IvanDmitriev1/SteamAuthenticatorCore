using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared;

public partial class AppSettings : ObservableObject, ISettings
{
    public AppSettings(ISettingsService settingsService, IPlatformImplementations platformImplementations)
    {
        _platformImplementations = platformImplementations;
        SettingsService = settingsService;
        IsInitialized = false;
    }

    private readonly IPlatformImplementations _platformImplementations;

    [ObservableProperty]
    private AccountsLocationModel _accountsLocation;
    
    [ObservableProperty]
    private bool _firstRun;
    
    [ObservableProperty]
    private bool _updated;
    
    [ObservableProperty]
    private int _periodicCheckingInterval;
    
    [ObservableProperty]
    private bool _autoConfirmMarketTransactions;

    [ObservableProperty]
    private Theme _theme;

    [IgnoreSetting]
    public bool IsInitialized { get; private set; }
    
    [IgnoreSetting]
    public ISettingsService SettingsService { get; }

    public void DefaultSettings()
    {
        AccountsLocation = AccountsLocationModel.LocalDrive;
        FirstRun = true;
        Updated = false;
        PeriodicCheckingInterval = 15;
        AutoConfirmMarketTransactions = false;
        Theme = Theme.System;

        IsInitialized = true;
    }
    
    public void LoadSettings()
    {
        DefaultSettings();
        SettingsService.LoadSettings(this);

        PropertyChanged += async (sender, args) =>
        {
            var settings = (sender as AppSettings)!;

            settings.SettingsService.SaveSetting(args.PropertyName!, settings);

            if (args.PropertyName != nameof(settings.Theme))
                return;

            await _platformImplementations.InvokeMainThread(() =>
            {
                _platformImplementations.SetTheme(settings.Theme);
            });
        };
    }

    public void SaveSettings()
    {
        SettingsService.SaveSettings(this);
    }
}