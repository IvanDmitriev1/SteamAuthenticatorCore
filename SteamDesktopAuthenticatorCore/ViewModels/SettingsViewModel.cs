using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SteamDesktopAuthenticatorCore.Common;
using WpfHelper.Commands;
using WpfHelper.Common;
using WpfHelper.Services;
using WPFUI.Common;
using WPFUI.Controls;
using BaseViewModel = WPFUI.Common.BaseViewModel;
using MessageBox = WPFUI.Controls.MessageBox;

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
                     await _dialog.ShowDialog("You are using the latest version", App.Name);
                     return;
                 }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await _dialog.ShowDialog("failed to check for updates", App.Name);

                return;
            }

            if (await _dialog.ShowDialog($"Would you like to download new version {model.NewVersion} ?", App.Name, "Yes", "No") != ButtonPressed.Left)
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
                await _dialog.ShowDialog( "Failed to download and install update", App.Name);
            }
        });
    }
}
