using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SteamAuthenticatorCore.Shared;

public partial class AppSettings : ObservableObject, ISettings
{
    public AppSettings(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        IsInitialized = false;
    }
    
    public enum ManifestLocationModel
    {
        LocalDrive,
        GoogleDrive
    }
    
    private readonly ISettingsService _settingsService;
    
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
        _settingsService.LoadSettings(this);
    }

    public async Task LoadSettingsAsync()
    {
        DefaultSettings();
        await _settingsService.LoadSettingsAsync(this);
    }
    
    public void SaveSettings()
    {
        _settingsService.SaveSettings(this);
    }

    public async Task SaveSettingsAsync()
    {
        await _settingsService.SaveSettingsAsync(this);
    }
}