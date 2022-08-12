using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.Views;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace SteamAuthenticatorCore.Desktop;

public sealed partial class App : Application
{
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

                services.AddSingleton<Container>();
                services.AddTransient<TokenPage>();
                services.AddTransient<ConfirmationsPage>();
                services.AddTransient<SettingsPage>();
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