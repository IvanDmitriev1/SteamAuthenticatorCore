using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.ViewModels;
using SteamAuthenticatorCore.Desktop.Views;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace SteamAuthenticatorCore.Desktop;

public sealed partial class App : Application
{
    public const string InternalName = "SteamDesktopAuthenticatorCore";
    public const string Name = "Steam desktop authenticator core";

    public App()
    {
        _host = Host
            .CreateDefaultBuilder()
            .ConfigureLogging((context, builder) =>
            {
                builder.AddConfiguration(context.Configuration);
                builder.AddDebug();
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
    }

    private readonly IHost _host;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.StartAsync();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _host.StopAsync();
        _host.Dispose();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        
    }
}