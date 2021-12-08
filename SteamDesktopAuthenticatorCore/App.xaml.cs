using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GoogleDrive;
using SteamAuthCore.Manifest;
using SteamDesktopAuthenticatorCore.classes;
using SteamDesktopAuthenticatorCore.Services;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore
{
    public sealed partial class App : Application
    {
        public App()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appdata, $"{Name}");
            string userCredentialPath = Path.Combine(appFolder, "Token.json");

            this.Dispatcher.UnhandledException += DispatcherOnUnhandledException;

            GoogleDriveApi = new GoogleDriveApi(userCredentialPath,
                new []{ Google.Apis.Drive.v3.DriveService.Scope.DriveFile },$"{Name}");

            UpdateService.GitHubUrl = "https://api.github.com/repos/bduj1/StreamDesktopAuthenticatorCore/releases/latest";
        }

        static App()
        {
            ManifestDirectoryService = new DesktopManifestDirectoryService();
        }

        #region Fields

        public static bool InDesignMode { get; private set; } = true;
        public static GoogleDriveApi GoogleDriveApi { get; private set; } = null!;
        public static IManifestModelService ManifestModelService { get; private set; } = null!;

        public const string Name = "SteamDesktopAuthenticatorCore";

        #endregion

        #region Variables

        private static readonly IManifestDirectoryService ManifestDirectoryService;

        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            InDesignMode = false;
            var settings = Settings.GetSettings();

            await Task.Run(CheckProcess);

            if (settings.Updated)
            {
                await UpdateService.DeletePreviousFile($"{Name}");

                settings.Updated = false;
                settings.SaveSettings();
            }

            base.OnStartup(e);
        }

        public static async Task InitializeManifestService()
        {
            var settings = Settings.GetSettings();
            switch (settings.ManifestLocation)
            {
                case Settings.ManifestLocationModel.Drive:
                    ManifestModelService = new LocalDriveManifestModelService(ManifestDirectoryService);
                    break;
                case Settings.ManifestLocationModel.GoogleDrive:
                    ManifestModelService = new GoogleDriveManifestModelService();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await ManifestModelService.Initialize();
        }

        #region PrivateMethods

        private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            MessageBox.Show( $"{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        private static void CheckProcess()
        {
            Process thisProcess = Process.GetCurrentProcess();
            foreach (var process in (from p in Process.GetProcesses() where p.ProcessName.Contains(Name) select p))
            {
                if (thisProcess.Id == process.Id) continue;

                if (MessageBox.Show("Another of this app running\nClose previous app instance ?\nIf no this app would be closed", "Process manager", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    process.Kill();
                else
                    App.Current.Shutdown();
            }
        }

        #endregion
    }
}
