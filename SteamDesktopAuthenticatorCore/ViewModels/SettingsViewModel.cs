using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared;
using WPFUI.Common;
using WPFUI.DIControls;
using WPFUI.DIControls.Interfaces;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class SettingsViewModel
{
    public SettingsViewModel(AppSettings appSettings, UpdateService updateService, Dialog dialog)
    {
        _updateService = updateService;
        _dialog = dialog;
        AppSettings = appSettings;

        CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    }

    private readonly UpdateService _updateService;
    private readonly Dialog _dialog;

    public AppSettings AppSettings { get; }

    public string CurrentVersion { get; }

    [ICommand]
    private async Task CheckForUpdates()
    {
        UpdateService.CheckForUpdateModel model;

        try
        {
            model = await _updateService.CheckForUpdate("SteamAuthenticatorCore.Desktop.exe");
            if (!model.NeedUpdate)
            {
                await _dialog.ShowDialog("You are using the latest version");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            await _dialog.ShowDialog("Failed to check for updates");

            return;
        }

        if (await _dialog.ShowDialog($"Would you like to download new version {model.NewVersion} ?", new DialogConfiguration()
            {
                Title = App.Name,
                LeftButtonText = "Yes",
                RightButtonText = "No",
            }) != ButtonPressed.Left)
            return;

        try
        {
            if (await _updateService.DownloadAndInstall(model) is not { } newFile)
                throw new ArgumentNullException();

            AppSettings.Updated = true;

            Application.Current.Shutdown(0);
            Process.Start(newFile);
        }
        catch
        {
            await _dialog.ShowDialog( "Failed to download and install update");
        }
    }
}