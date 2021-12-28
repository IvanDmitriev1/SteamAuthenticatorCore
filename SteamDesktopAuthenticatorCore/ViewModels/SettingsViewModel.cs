using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SteamDesktopAuthenticatorCore.Common;
using WpfHelper.Commands;
using WpfHelper.Common;
using WpfHelper.Services;
using WPFUI.Common;
using WPFUI.Controls;
using MessageBox = WPFUI.Controls.MessageBox;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel(SettingService settingsService, UpdateService updateService)
        {
            _updateService = updateService;
            AppSettings = settingsService.Get<AppSettings>();
        }

        private readonly UpdateService _updateService;

        public AppSettings AppSettings { get; }

        public ICommand CheckForUpdatesCommand => new AsyncRelayCommand(async o =>
        {
            UpdateService.CheckForUpdateModel model;
            var dialog = Dialog.GetCurrentInstance();

            try
            {
                 model = await _updateService.CheckForUpdate("SteamDesktopAuthenticatorCore.exe");
                 if (!model.NeedUpdate)
                 {
                     await dialog.ShowDialog("You are using the latest version", App.Name);
                     return;
                 }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await dialog.ShowDialog("failed to check for updates", App.Name);

                return;
            }

            if (await dialog.ShowDialog($"Would you like to download new version {model.NewVersion} ?", App.Name, "Yes", "No") != ButtonPressed.Left)
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
                await dialog.ShowDialog( "Failed to download and install update", App.Name);
            }
        });
    }
}
