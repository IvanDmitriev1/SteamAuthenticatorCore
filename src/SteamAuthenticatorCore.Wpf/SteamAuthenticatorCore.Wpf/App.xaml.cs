using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamAuthCore.Extensions;
using SteamAuthenticatorCore.Desktop.ViewModels;
using SteamAuthenticatorCore.Desktop.Views;

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
                services.AddSingleton<MainWindow>();

                services.AddTransient<TokenPage>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ConfirmationsOverviewPage>();

                services.AddTransient<AccountConfirmations>();
                services.AddTransient<LoginPage>();

                services.AddTransient<TokenViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<ConfirmationsOverviewViewModel>();

                services.AddSingleton<AccountConfirmationsViewModel>();
                services.AddSingleton<LoginViewModel>();

                services.AddSingleton<IPlatformImplementations, DesktopImplementations>();
                services.AddSingleton<LocalDriveAccountsService>();
                services.AddSingleton<GoogleDriveAccountsService>();

                services.AddSteamAuthCoreServices(false);
                services.AddSharedServices();

                services.AddSingleton<IUpdateService, UpdateService>(provider =>
                    new UpdateService(Assembly.GetExecutingAssembly().GetName().Version!));

                services.AddSingleton<AccountsServiceResolver>(static provider => () =>
                {
                    var appSettings = AppSettings.Current;

                    if (!GoogleDriveAccountsService.IsClientSecretAttachedToAssembly())
                        return provider.GetRequiredService<LocalDriveAccountsService>();

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
        WpfAppSettings.Current.Load();

        await _host.StartAsync();
        ServiceProvider = _host.Services;

        ServiceProvider.GetRequiredService<MainWindow>().Show();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        OnException(e.Exception, _logger);
    }
}