using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAuthenticatorCore.Shared;

public partial class AppSettings : ObservableObject, ISettings
{
    public AppSettings(ISettingsService settingsService)
    {
        SettingsService = settingsService;
        IsInitialized = false;
    }
    
    public enum ManifestLocationModel
    {
        LocalDrive,
        GoogleDrive
    }

    [ObservableProperty]
    private ManifestLocationModel _manifestLocation;
    
    [ObservableProperty]
    private bool _firstRun;
    
    [ObservableProperty]
    private bool _updated;
    
    [ObservableProperty]
    private int _periodicCheckingInterval;
    
    [ObservableProperty]
    private bool _autoConfirmMarketTransactions;

    [ObservableProperty]
    private Theme _appTheme;

    [IgnoreSettings]
    public bool IsInitialized { get; private set; }
    
    [IgnoreSettings]
    public ISettingsService SettingsService { get; }

    public void DefaultSettings()
    {
        ManifestLocation = ManifestLocationModel.LocalDrive;
        FirstRun = true;
        Updated = false;
        PeriodicCheckingInterval = 10;
        AutoConfirmMarketTransactions = false;
        AppTheme = Theme.System;

        IsInitialized = true;
    }
    
    public void LoadSettings()
    {
        DefaultSettings();
        SettingsService.LoadSettings(this);
    }

    public void SaveSettings()
    {
        SettingsService.SaveSettings(this);
    }
}