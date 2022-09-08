using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(AppSettings appSettings, IUpdateService updateService)
    {
        _updateService = updateService;
        AppSettings = appSettings;
        CurrentVersion = VersionTracking.CurrentVersion;

        _themeSelection = string.Empty;

        _themeSelection = AppSettings.Theme switch
        {
            Theme.System => "System",
            Theme.Light => "Light",
            Theme.Dark => "Dark",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private readonly IUpdateService _updateService;
    private string _themeSelection;
    
    public AppSettings AppSettings { get; }
    public string CurrentVersion { get; }

    public string ThemeSelection
    {
        get => _themeSelection;
        set
        {
            _themeSelection = value;
            AppSettings.Theme = Enum.Parse<Theme>(value);
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private async Task OnSettingsClicked(object? obj)
    {
        if (obj is Switch)
        {
            AppSettings.AutoConfirmMarketTransactions = !AppSettings.AutoConfirmMarketTransactions;
            return;
        }

        try
        {
            var value = await Application.Current!.MainPage!.DisplayPromptAsync("Settings", "Seconds between checking confirmations", "Change", "Cancel", string.Empty, 2, Keyboard.Numeric, AppSettings.PeriodicCheckingInterval.ToString());
            if (!int.TryParse(value, out var result))
                return;

            if (result < 15)
                return;

            AppSettings.PeriodicCheckingInterval = result;
        }
        catch (Exception)
        {
            //
        }
    }

    [RelayCommand]
    private void StoppedTyping()
    {
        if (AppSettings.PeriodicCheckingInterval < 15)
            AppSettings.PeriodicCheckingInterval = 15;
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await _updateService.CheckForUpdateAndDownloadInstall(false);
    }
}
