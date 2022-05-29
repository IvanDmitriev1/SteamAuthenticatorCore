using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
        AppSettings = DependencyService.Get<AppSettings>();

        _themeSelection = AppSettings.AppTheme switch
        {
            Theme.System => "System",
            Theme.Light => "Light",
            Theme.Dark => "Dark",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public AppSettings AppSettings { get; }

    private string _themeSelection;

    public string ThemeSelection
    {
        get => _themeSelection;
        set
        {
            _themeSelection = value;
            AppSettings.AppTheme = Enum.Parse<Theme>(value);
            OnPropertyChanged(nameof(ThemeSelection));
        }
    }

    [ICommand]
    private void OnSettingsClicked(object obj)
    {
        switch (obj)
        {
            case Switch:
                AppSettings.AutoConfirmMarketTransactions = !AppSettings.AutoConfirmMarketTransactions;
                return;
            case Entry entry:
                entry.Focus();
                return;
        }
    }

    [ICommand]
    private void StoppedTyping()
    {
        if (AppSettings.PeriodicCheckingInterval < 10)
            AppSettings.PeriodicCheckingInterval = 10;
    }
}