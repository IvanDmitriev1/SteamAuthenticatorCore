using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
        AppSettings = MauiAppSettings.Current;

        _themeSelection = AppSettings.Theme switch
        {
            AppTheme.Unspecified => "System",
            AppTheme.Light => "Light",
            AppTheme.Dark => "Dark",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private readonly IUpdateService _updateService;

    public MauiAppSettings AppSettings { get; }

    [ObservableProperty]
    private string _themeSelection;

    partial void OnThemeSelectionChanged(string value)
    {
        AppSettings.Theme = value switch
        {
            "System" => AppTheme.Unspecified,
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [RelayCommand]
    private void ChangeAutoConfirmation()
    {
        AppSettings.AutoConfirmMarketTransactions = !AppSettings.AutoConfirmMarketTransactions;
    }

    [RelayCommand]
    private async Task ChangeCheckingIntervalPrompt()
    {
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
