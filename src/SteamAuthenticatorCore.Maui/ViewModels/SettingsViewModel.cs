﻿using CommunityToolkit.Maui.Views;
using SteamAuthenticatorCore.Maui.Popups;

namespace SteamAuthenticatorCore.Maui.ViewModels;

public sealed partial class SettingsViewModel : MyObservableRecipient
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

    [RelayCommand]
    private Task NavigateToAvailableLanguagesPage()
    {
        return Shell.Current.GoToAsync(nameof(AvailableLanguagesPage));
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
            var value = await Application.Current!.MainPage!.DisplayPromptAsync(
                AppSettings.LocalizationProvider[LocalizationMessage.Settings],
                AppSettings.LocalizationProvider[LocalizationMessage.SecondsBetweenCheckingForConfirmationsMessage],
                AppSettings.LocalizationProvider[LocalizationMessage.ChangeMessage],
                AppSettings.LocalizationProvider[LocalizationMessage.CancelMessage], string.Empty, 2, Keyboard.Numeric,
                AppSettings.PeriodicCheckingInterval.ToString());

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
        try
        {
            if (await _updateService.CheckForUpdate() is not { } release)
            {
                await Toast.Make(AppSettings.LocalizationProvider[LocalizationMessage.YouAreUsingTheLatestVersionMessage], ToastDuration.Long, 16).Show();
                return;
            }

            var updatePopup = new UpdatePopup(release);
            await Shell.Current.CurrentPage.ShowPopupAsync(updatePopup);
        }
        catch
        {
            //
        }
    }

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
}
