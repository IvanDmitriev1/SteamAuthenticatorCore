using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using System.Reflection;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
        AppSettings = AppSettings.Current;
        CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
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
