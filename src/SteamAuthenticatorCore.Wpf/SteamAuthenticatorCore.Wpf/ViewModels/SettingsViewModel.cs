using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using SteamAuthenticatorCore.Desktop.Helpers;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.IconElements;
using SteamAuthenticatorCore.Desktop.Dialogs;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(IUpdateService updateService, ILogger<SettingsViewModel> logger)
    {
        _updateService = updateService;
        _logger = logger;
        AppSettings = AppSettings.Current;
        CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    }

    private readonly IUpdateService _updateService;
    private readonly ILogger<SettingsViewModel> _logger;

    public AppSettings AppSettings { get; }
    public string CurrentVersion { get; }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        try
        {
            if (await _updateService.CheckForUpdate() is not { } release)
            {
                await SnackbarService.Default.ShowAsync("Updater", "You are using the latest version", new SymbolIcon(SymbolRegular.Info24));
                return;
            }

            var contentDialog = new DownloadUpdateContentDialog(ContentDialogService.Default.GetContentPresenter(), release);
            await contentDialog.ShowAsync();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"{nameof(CheckForUpdates)} method");
        }
    }
}
