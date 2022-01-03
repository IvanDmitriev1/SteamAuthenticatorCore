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

        #region Overrides

        protected override async void OnStartup(StartupEventArgs e)
        {
            WPFUI.Theme.Manager.SetSystemTheme(false);

            await Host.StartAsync();

            var settingsService = Host.Services.GetRequiredService<SettingService>();
            settingsService.LoadSettings();

            var mainWindow = Host.Services.GetRequiredService<Container>();
            mainWindow.Show();

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
            service.Configure<UpdateServiceOptions>(options =>
            {
                options.GitHubUrl = "https://api.github.com/repos/bduj1/StreamDesktopAuthenticatorCore/releases/latest";
            });

            service.Configure<DefaultNavigationConfiguration>(configuration =>
            {
                configuration.StartupPageTag = nameof(TokenPage);

                configuration.Items = new Dictionary<string, INavigationItem>()
                {
                    {nameof(TokenPage), new DefaultNavigationItem(typeof(TokenPage), "Token")},
                    {nameof(ConfirmationsPage), new DefaultNavigationItem(typeof(ConfirmationsPage), "Confirmations")},


                    {
                        nameof(SettingsPage), new DefaultNavigationItem(typeof(SettingsPage), "Settings", Icon.Settings24)
                        {
                            Footer = true
                        }
                    },
                };
            });
        }

        private static void ConfigureServices(IServiceCollection service)
        {
            service.AddSingleton<Container>();
            service.AddSingleton<Dialog>();
            service.AddSingleton<Snackbar>();
            service.AddSingleton<DefaultNavigation>();

            service.AddScoped<TokenViewModel>();
            service.AddScoped<SettingsViewModel>();
            service.AddScoped<ConfirmationViewModel>();

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
