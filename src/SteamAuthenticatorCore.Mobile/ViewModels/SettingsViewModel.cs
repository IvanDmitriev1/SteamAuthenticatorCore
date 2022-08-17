using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Models;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(AppSettings appSettings)
    {
        AppSettings = appSettings;

        _themeSelection = string.Empty;

        _themeSelection = AppSettings.Theme switch
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
            AppSettings.Theme = Enum.Parse<Theme>(value);
            OnPropertyChanged(nameof(ThemeSelection));
        }
    }

    [RelayCommand]
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

    [RelayCommand]
    private void StoppedTyping()
    {
        if (AppSettings.PeriodicCheckingInterval < 10)
            AppSettings.PeriodicCheckingInterval = 10;
    }
}