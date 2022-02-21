using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SteamDesktopAuthenticatorCore.Common;
using WpfHelper.Commands;
using WpfHelper.Services;
using WPFUI.Common;
using WPFUI.Controls;
using BaseViewModel = WPFUI.Common.BaseViewModel;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class SettingsViewModel : BaseViewModel
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


        public ICommand CheckForUpdatesCommand => new AsyncRelayCommand(async o =>
        {
            UpdateService.CheckForUpdateModel model;

            try
            {
                 model = await _updateService.CheckForUpdate("SteamDesktopAuthenticatorCore.exe");
                 if (!model.NeedUpdate)
                 {
                     await _dialog.ShowDialog(App.Name, "You are using the latest version");
                     return;
                 }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await _dialog.ShowDialog(App.Name, "failed to check for updates");

                return;
            }

            if (await _dialog.ShowDialog(App.Name, $"Would you like to download new version {model.NewVersion} ?", "Yes", "No") != ButtonPressed.Left)
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
                await _dialog.ShowDialog( App.Name, "Failed to download and install update");
            }
        });
    }
}
