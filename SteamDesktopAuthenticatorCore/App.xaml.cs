using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.ViewModels;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using WpfHelper.Services;
using WPFUI.Common;
using WPFUI.DIControls;
using WPFUI.DIControls.Interfaces;


namespace SteamAuthenticatorCore.Desktop
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

        public delegate IManifestModelService ManifestServiceResolver();

        public const string InternalName = "SteamDesktopAuthenticatorCore";
        public const string Name = "Steam desktop authenticator core";

        private readonly IHost _host;

        #region Overrides

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var services = _host.Services;

            var confirmationService = services.GetRequiredService<BaseConfirmationService>();

            var appSettings = services.GetRequiredService<AppSettings>();
            appSettings.PropertyChanged += AppSettingsOnPropertyChanged;


            var settings = services.GetRequiredService<AppSettings>();
            settings.LoadSettings();

            await OnStartupTask(_host.Services);

            var mainWindow = _host.Services.GetRequiredService<Views.Container>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _host.Services.GetRequiredService<AppSettings>().SaveSettings();

            var appSettings = _host.Services.GetRequiredService<AppSettings>();
            appSettings.PropertyChanged -= AppSettingsOnPropertyChanged;

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

                configuration.HiddenItemsItems = new Dictionary<string, INavigationItem>()
                {
                    {nameof(LoginPage), new DefaultNavigationItem(typeof(LoginPage), "Login")},
                    {nameof(CaptchaPage), new DefaultNavigationItem(typeof(CaptchaPage), "Captcha")},
                };
            });
        }

        private static void ConfigureServices(IServiceCollection service)
        {
            service.AddSingleton<Views.Container>();
            service.AddScoped<Dialog>();
            service.AddScoped<Snackbar>();
            service.AddScoped<DefaultNavigation>();

            service.AddSingleton<TokenViewModel>();
            service.AddSingleton<ConfirmationViewModel>();
            service.AddTransient<SettingsViewModel>();
            service.AddTransient<LoginViewModel>();
            service.AddTransient<CaptchaViewModel>();

            service.AddTransient<TokenPage>();
            service.AddTransient<SettingsPage>();
            service.AddTransient<LoginPage>();
            service.AddTransient<ConfirmationsPage>();
            service.AddTransient<CaptchaPage>();

            service.AddScoped<IPlatformImplementations, DesktopImplementations>();
            service.AddScoped<BaseConfirmationService, DesktopConfirmationService>();
            service.AddTransient<IPlatformTimer, DesktopTimer>();
            
            service.AddScoped<TokenService>();

            service.AddSingleton<SimpleHttpRequestService>();
            service.AddSingleton<UpdateService>();
            service.AddGoogleDriveApi(Name);
            service.AddScoped<AppSettings>();
            service.AddScoped<ISettingsService, DesktopSettingsService>();

            service.AddScoped<GoogleDriveManifestModelService>();
            service.AddScoped<IManifestDirectoryService, DesktopManifestDirectoryService>();
            service.AddScoped<LocalDriveManifestModelService>();

            service.AddSingleton<ObservableCollection<SteamGuardAccount>>();

            service.AddTransient<ManifestServiceResolver>(provider => () =>
            {
                var appSettings = provider.GetRequiredService<AppSettings>();
                return appSettings.ManifestLocation switch
                {
                    AppSettings.ManifestLocationModel.LocalDrive => provider
                        .GetRequiredService<LocalDriveManifestModelService>(),
                    AppSettings.ManifestLocationModel.GoogleDrive => provider
                        .GetRequiredService<GoogleDriveManifestModelService>(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
        }

        private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            MessageBox.Show( $"{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevent default unhandled exception processing
            e.Handled = true;

            Application.Current.Shutdown();
        }

        private static async Task OnStartupTask(IServiceProvider services)
        {
            var appSettings = services.GetRequiredService<AppSettings>();
            if (appSettings.Updated)
            {
                var updateService = services.GetRequiredService<UpdateService>();
                await updateService.DeletePreviousFile(InternalName);
                appSettings.Updated = false;
            }
        }

        private async void AppSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var settings = (AppSettings) sender!;
            if (!settings.IsInitialized) return;

            if (e.PropertyName != nameof(settings.ManifestLocation)) return;

            var tokenViewModel = _host.Services.GetRequiredService<TokenViewModel>();
            await tokenViewModel.OnManifestLocationChanged();
        }

        #endregion
    }
}
