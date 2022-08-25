using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class SettingsViewModel
{
    public SettingsViewModel(AppSettings settings, IUpdateService updateService)
    {
        _updateService = updateService;
        Settings = settings;
        CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    }

    private readonly IUpdateService _updateService;
    public AppSettings Settings { get; }
    public string CurrentVersion { get; }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await _updateService.CheckForUpdateAndDownloadInstall(false);
    }
}
