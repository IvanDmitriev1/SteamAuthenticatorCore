using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Extensions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.ViewModels;
using SteamAuthenticatorCore.Desktop.Views;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Extensions;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Contracts;

namespace SteamAuthenticatorCore.Desktop;

public sealed partial class App : Application
{
    public const string InternalName = "SteamDesktopAuthenticatorCore";
    public const string Name = "Steam desktop authenticator core";
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    private readonly ILogger<App> _logger;

    public App()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        _host = Host
            .CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder =>
            {
                var appName = Assembly.GetEntryAssembly()!.GetName().Name;

                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{appName}.appsettings.json");
                builder.AddJsonStream(stream!);
            })
            .ConfigureLogging((context, builder) =>
            {
                builder.AddConfiguration(context.Configuration);

#if DEBUG
                builder.AddDebug();
#else
                builder.AddSentry();
#endif
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ObservableCollection<SteamGuardAccount>>();
                services.AddSingleton<MainWindow>();

                services.AddTransient<TokenPage>();
                services.AddTransient<ConfirmationsOverviewPage>();
                services.AddTransient<ConfirmationsPage>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<LoginPage>();

                services.AddTransient<TokenViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<ConfirmationsOverviewViewModel>();
                services.AddTransient<ConfirmationsViewModel>();
                services.AddTransient<LoginViewModel>();

                services.AddGoogleDriveApi(Name);
                services.AddSingleton<IPlatformImplementations, DesktopImplementations>();

                services.AddSingleton<AppSettings>(WpfAppSettings.Current);
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                services.AddScoped<LocalDriveAccountsService>();
                services.AddScoped<GoogleDriveAccountsService>();

                services.AddSteamAuthCoreServices();
                services.AddSharedServices();

                services.AddSingleton<IUpdateService, UpdateService>(provider =>
                    new UpdateService(Assembly.GetExecutingAssembly().GetName().Version!));

                services.AddSingleton<AccountsServiceResolver>(provider => () =>
                {
                    var appSettings = provider.GetRequiredService<AppSettings>();
                    return appSettings.AccountsLocation switch
                    {
                        AccountsLocation.LocalDrive =>
                            provider.GetRequiredService<LocalDriveAccountsService>(),
                        AccountsLocation.GoogleDrive =>
                            provider.GetRequiredService<GoogleDriveAccountsService>(),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                });
            })
            .Build();

        _logger = _host.Services.GetRequiredService<ILogger<App>>();
    }

    private readonly IHost _host;

    public static void OnException(Exception exception, ILogger logger)
    {
        logger.LogCritical(exception, "Exception occurred");

        MessageBox.Show( $"{exception.Message}\n\n{exception.StackTrace}", "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);

        Application.Current.Shutdown();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    { 
        await _host.StartAsync();
        ServiceProvider = _host.Services;

        await ServiceProvider.GetRequiredService<ITimeAligner>().AlignTimeAsync();
        await ServiceProvider.GetRequiredService<AccountsServiceResolver>().Invoke().InitializeOrRefresh();

        ServiceProvider.GetRequiredService<MainWindow>().Show();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        OnException(e.Exception, _logger);
    }
}