using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class SettingsViewModel : SettingsViewModelBase
{
    public SettingsViewModel(AppSettings appSettings, IUpdateService updateService) : base(appSettings, updateService, VersionTracking.CurrentVersion)
    {
        _themeSelection = AppSettings.Theme switch
        {
            Theme.System => "System",
            Theme.Light => "Light",
            Theme.Dark => "Dark",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string _themeSelection;
    
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
}
