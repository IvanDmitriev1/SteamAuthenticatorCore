using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamAuthCore.Manifest;
using SteamDesktopAuthenticatorCore.Common;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.ViewModels;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore
{
    public sealed partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += DispatcherOnUnhandledException;

            
        }

        static App()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().ConfigureServices((context, collection) =>
            {
                ConfigureOptions(collection);
                ConfigureServices(collection);
            }).Build();
        }

        public const string InternalName = "SteamDesktopAuthenticatorCore";

        public const string Name = "Steam desktop authenticator core";
        private static readonly IHost Host;

        public static IServiceProvider ServiceProvider => Host.Services;

        #region Overrides

        protected override async void OnStartup(StartupEventArgs e)
        {
            WPFUI.Theme.Manager.SetSystemTheme(false);

            await Host.StartAsync();

            var appSettings = Host.Services.GetRequiredService<SettingService>().Get<AppSettings>();
            switch (appSettings.ManifestLocation)
            {
                case AppSettings.ManifestLocationModel.Drive:
                    var mainWindow = Host.Services.GetRequiredService<Container>();
                    mainWindow.Show();
                    break;
                case AppSettings.ManifestLocationModel.GoogleDrive:
                    var initializingWindow = Host.Services.GetRequiredService<InitializingWindow>();
                    initializingWindow.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Host.Services.GetRequiredService<SettingService>().SaveSettings();

            await Host.StopAsync();
            Host.Dispose();

            base.OnExit(e);
        }

        #endregion

        #region PrivateMethods

        private static void ConfigureOptions(IServiceCollection service)
        {
            service.Configure<SettingsServiceOptions>(options =>
            {
                options.AppName = InternalName;
                options.SettingsMap = new Dictionary<Type, ISettings>()
                {
                    {typeof(AppSettings), new AppSettings()}
                };
            });
            service.Configure<UpdateServiceOptions>(options =>
            {
                options.GitHubUrl = "https://api.github.com/repos/bduj1/StreamDesktopAuthenticatorCore/releases/latest";
            });
        }

        private static void ConfigureServices(IServiceCollection service)
        {
            service.AddSingleton<InitializingWindow>();
            service.AddSingleton<Container>();

            service.AddSingleton<InitializingViewModel>();
            service.AddSingleton<TokenViewModel>();
            service.AddSingleton<SettingsViewModel>();
            service.AddSingleton<ConfirmationViewModel>();

            service.AddSingleton<SimpleHttpRequestService>();
            service.AddSingleton<UpdateService>();
            service.AddSingleton<SettingService>();
            service.AddGoogleDriveApi(Name);


            service.AddSingleton<GoogleDriveManifestModelService>();

            service.AddSingleton<IManifestDirectoryService, DesktopManifestDirectoryService>();
            service.AddSingleton<LocalDriveManifestModelService>();

            service.AddTransient<ManifestServiceResolver>(provider => () => 
            {
                var appSettings = provider.GetRequiredService<SettingService>().Get<AppSettings>();
                return appSettings.ManifestLocation switch
                {
                    AppSettings.ManifestLocationModel.Drive => provider
                        .GetRequiredService<LocalDriveManifestModelService>(),
                    AppSettings.ManifestLocationModel.GoogleDrive => provider
                        .GetRequiredService<GoogleDriveManifestModelService>(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
        }

        public delegate IManifestModelService ManifestServiceResolver();

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
