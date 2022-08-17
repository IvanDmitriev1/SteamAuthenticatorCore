using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.ViewModels;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Container = SteamAuthenticatorCore.Desktop.Views.Container;

namespace SteamAuthenticatorCore.Desktop;

public sealed partial class App : Application
{
    public const string InternalName = "SteamDesktopAuthenticatorCore";
    public const string Name = "Steam desktop authenticator core";
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    private IServiceScope? _serviceScope;

    public App()
    {
        _host = Host
            .CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder =>
            {
                var appName = Assembly.GetEntryAssembly()!.GetName().Name;

                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{appName}.appsettings.json");
                builder.AddJsonStream(stream);
            })
            .ConfigureLogging((context, builder) =>
            {
                builder.AddConfiguration(context.Configuration);
                builder.AddDebug();

                builder.AddSentry(options =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                        options.InitializeSdk = false;
                });
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<ApplicationHostService>();

                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<ITaskBarService, TaskBarService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<TaskBarServiceWrapper>();

                services.AddSingleton<Container>();
                services.AddTransient<TokenPage>();
                services.AddTransient<ConfirmationsPage>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<LoginPage>();

                services.AddScoped<TokenViewModel>();
                services.AddScoped<SettingsViewModel>();
                services.AddScoped<ConfirmationsViewModel>();
                services.AddScoped<LoginViewModel>();

                services.AddSingleton<ObservableCollection<SteamGuardAccount>>();

                services.AddSingleton<AppSettings>();
                services.AddGoogleDriveApi(Name);
                services.AddTransient<ISettingsService, DesktopSettingsService>();
                services.AddSingleton<IPlatformImplementations, DesktopImplementations>();

                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                services.AddScoped<GoogleDriveManifestModelService>();
                services.AddScoped<IManifestDirectoryService, DesktopManifestDirectoryService>();
                services.AddScoped<LocalDriveManifestModelService>();
                services.AddScoped<ManifestAccountsWatcherService>();
                services.AddTransient<IPlatformTimer, PeriodicTimerService>();
                services.AddScoped<ConfirmationServiceBase, DesktopConfirmationService>();
                services.AddScoped<LoginService>();
                
                services.AddScoped<ManifestServiceResolver>(provider => () =>
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
            })
            .Build();

        var settings = _host.Services.GetRequiredService<AppSettings>();
        settings.PropertyChanged += SettingsOnPropertyChanged;
    }

    private readonly IHost _host;

    public static void OnException(Exception exception, IHub hub)
    {
        hub.CaptureException(exception);

        MessageBox.Show( $"{exception.Message}\n\n{exception.StackTrace}", "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);

        Application.Current.Shutdown();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var environment = _host.Services.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            _serviceScope = _host.Services.CreateScope();
            ServiceProvider = _serviceScope.ServiceProvider;
        }
        else
            ServiceProvider = _host.Services;

        await _host.StartAsync();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _host.StopAsync();

        _serviceScope?.Dispose();
        _host.Dispose();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        var hub = _host.Services.GetRequiredService<IHub>();
        OnException(e.Exception, hub);
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (sender as AppSettings)!;

        settings.SettingsService.SaveSetting(e.PropertyName!, settings);
    }
}