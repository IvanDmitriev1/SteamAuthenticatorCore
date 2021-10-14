using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using GoogleDrive;
using SteamDesktopAuthenticatorCore.Services;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore
{
    public partial class App : Application, IDisposable
    {
        public App()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appdata, $"{Name}");
            string userCredentialPath = Path.Combine(appFolder, "Token.json");

            GoogleDriveApi = new GoogleDriveApi(userCredentialPath,
                new []{ Google.Apis.Drive.v3.DriveService.Scope.DriveFile },$"{Name}");

            ManifestModelService.Api = GoogleDriveApi;

            UpdateService.GitHubUrl = "https://api.github.com/repos/bduj1/StreamDesktopAuthenticatorCore/releases/latest";
        }

        #region Fields
        public static bool InDesignMode { get; private set; } = true;
        public static GoogleDriveApi GoogleDriveApi { get; private set; } = null!;

        public const string Name = "SteamDesktopAuthenticatorCore";

        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            InDesignMode = false;

            CheckProcess();

            await UpdateService.DeletePreviousFile("SteamDesktopAuthenticatorCore");

            base.OnStartup(e);
        }

        #region PrivateMethods

        private static void CheckProcess()
        {
            Process thisProcess = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(thisProcess.ProcessName))
            {
                if (thisProcess.Id == process.Id) continue;

                if (MessageBox.Show("Another of this app running\nClose previous app instance ?\nIf no this app would be closed", "Process manager", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    process.Kill();
                else
                    App.Current.Shutdown();
            }
        }

        #endregion

        public void Dispose()
        {
            GoogleDriveApi.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
