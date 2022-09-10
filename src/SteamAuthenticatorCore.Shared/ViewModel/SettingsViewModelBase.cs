using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class SettingsViewModelBase : ObservableObject
{
    protected SettingsViewModelBase(AppSettings appSettings, IUpdateService updateService, string currentVersion)
    {
        _updateService = updateService;
        AppSettings = appSettings;
        CurrentVersion = currentVersion;
    }

    private readonly IUpdateService _updateService;

    public AppSettings AppSettings { get; }
    public string CurrentVersion { get; }

    
    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await _updateService.CheckForUpdateAndDownloadInstall(false);
    }
}
