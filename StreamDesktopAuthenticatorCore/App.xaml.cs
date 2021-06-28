using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using GoogleDrive;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Services;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore
{
    public partial class App : Application, IDisposable
    {
        public App()
        {
            UserCredentialPath = $"{Path.GetTempPath()}\\SteamDesktopAuthenticatorCoreToken.json";
            GoogleDriveApi = new GoogleDriveApi(UserCredentialPath,
                new []{ Google.Apis.Drive.v3.DriveService.Scope.DriveFile },
                "SteamDesktopAuthenticatorCore");

            UpdateService.GitHubUrl = "https://api.github.com/repos/bduj1/StreamDesktopAuthenticatorCore/releases/latest";
        }

        #region Fields
        public static bool InDesignMode { get; private set; } = true;
        public static GoogleDriveApi GoogleDriveApi { get; private set; } = null!;
        private static string UserCredentialPath { get; set; } = null!;

        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            InDesignMode = false;

            CheckProcess();

            await DeletePreviousFile();

            await StartWindow();

            base.OnStartup(e);
        }

        #region ProvateMethods

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

        private static async Task DeletePreviousFile()
        {
            await Task.Delay(5000);

            string[] files = await Task.Run(() => Directory.GetFiles(Environment.CurrentDirectory));
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);

                if (!fileName.Contains("SteamDesktopAuthenticatorCore") || !fileName.Contains("exe")) continue;
                if (fileName == Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)) continue;

                try
                {
                    File.Delete(file);
                }
                catch
                {
                    Debug.WriteLine("Failed to delete file");
                }
            }
        }

        private async Task StartWindow()
        {
            ManifestModel manifest = await ManifestModelService.GetManifestFromGoogleDrive();
        }

        #endregion

        public void Dispose()
        {
            GoogleDriveApi.Dispose();
        }
    }
}
