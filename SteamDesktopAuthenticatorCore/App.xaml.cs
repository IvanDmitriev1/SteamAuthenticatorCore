using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using SteamDesktopAuthenticatorCore.Common;
using SteamDesktopAuthenticatorCore.ViewModels;
using WpfHelper.Common;

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
        }

        static App()
        {
            ViewModels = new Dictionary<Type, BaseViewModel>()
            {
                {typeof(InitializingViewModel), new InitializingViewModel()},
            };
        }

        #region Fields

        public const string Name = "SteamDesktopAuthenticatorCore";
        public static IReadOnlyDictionary<Type, BaseViewModel> ViewModels { get; }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            WPFUI.Theme.Manager.SetSystemTheme(false);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.GetSettings().SaveSettings();
        }

        #region PrivateMethods

        private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            MessageBox.Show( $"{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
        #endregion
    }
}
