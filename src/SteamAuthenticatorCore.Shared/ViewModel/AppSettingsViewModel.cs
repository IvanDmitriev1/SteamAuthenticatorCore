using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public partial class AppSettingsViewModel : ObservableObject, ISettings
{
    public AppSettingsViewModel(ISettingsService settingsService)
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