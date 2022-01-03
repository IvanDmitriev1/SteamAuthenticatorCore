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
using SteamDesktopAuthenticatorCore.Views.Pages;
using WpfHelper.Services;
using WPFUI.Controls;
using WPFUI.Controls.Navigation;
using Icon = WPFUI.Common.Icon;
using MessageBox = System.Windows.MessageBox;

namespace SteamDesktopAuthenticatorCore
{
    public sealed partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += DispatcherOnUnhandledException;

            _host = Host.CreateDefaultBuilder().ConfigureServices((context, collection) =>
            {
                ConfigureOptions(collection);
                ConfigureServices(collection);
            }).Build();
        }

        public const string InternalName = "SteamDesktopAuthenticatorCore";

        public const string Name = "Steam desktop authenticator core";
        private readonly IHost _host;

        #region Overrides

        protected override async void OnStartup(StartupEventArgs e)
        {
            WPFUI.Theme.Manager.SetSystemTheme(false);

            await _host.StartAsync();

            var settingsService = _host.Services.GetRequiredService<SettingService>();
            settingsService.LoadSettings();

            var mainWindow = _host.Services.GetRequiredService<Container>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _host.Services.GetRequiredService<SettingService>().SaveSettings();

            await _host.StopAsync();
            _host.Dispose();
        }

        #endregion

        #region PrivateMethods

        private static void ConfigureOptions(IServiceCollection service)
        {
            service.Configure<UpdateServiceOptions>(options =>
            {
                options.GitHubUrl = "https://api.github.com/repos/bduj1/StreamDesktopAuthenticatorCore/releases/latest";
            });

            service.Configure<DefaultNavigationConfiguration>(configuration =>
            {
                configuration.StartupPageTag = nameof(TokenPage);

                configuration.VisableItems = new Dictionary<string, INavigationItem>()
                {
                    {nameof(TokenPage), new DefaultNavigationItem(typeof(TokenPage), "Token")},
                    {nameof(ConfirmationsPage), new DefaultNavigationItem(typeof(ConfirmationsPage), "Confirmations")},
                };

                configuration.VisableFooterItems = new Dictionary<string, INavigationItem>()
                {
                    {nameof(SettingsPage), new DefaultNavigationItem(typeof(SettingsPage), "Settings", Icon.Settings24)}
                };
            });
        }

        private static void ConfigureServices(IServiceCollection service)
        {
            service.AddSingleton<Container>();
            service.AddSingleton<Dialog>();
            service.AddSingleton<Snackbar>();
            service.AddSingleton<DefaultNavigation>();

            service.AddSingleton<TokenViewModel>();
            service.AddSingleton<SettingsViewModel>();
            service.AddSingleton<ConfirmationViewModel>();

            service.AddTransient<TokenPage>();
            service.AddTransient<SettingsPage>();
            service.AddTransient<LoginPage>();
            service.AddTransient<ConfirmationsPage>();

            service.AddSingleton<SimpleHttpRequestService>();
            service.AddSingleton<UpdateService>();
            service.AddGoogleDriveApi(Name);
            service.AddSettings(InternalName);

            service.AddSingleton<GoogleDriveManifestModelService>();
            service.AddSingleton<IManifestDirectoryService, DesktopManifestDirectoryService>();
            service.AddSingleton<LocalDriveManifestModelService>();

            service.AddTransient<ManifestServiceResolver>(provider => () =>
            {
                var appSettings = provider.GetRequiredService<AppSettings>();
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
