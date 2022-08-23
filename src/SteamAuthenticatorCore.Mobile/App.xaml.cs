using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        _environment = Startup.ServiceProvider.GetRequiredService<IEnvironment>();
        
        var settings = Startup.ServiceProvider.GetRequiredService<AppSettings>();
        settings.LoadSettings();

        var platformImplementations = Startup.ServiceProvider.GetRequiredService<IPlatformImplementations>();
        platformImplementations.SetTheme(settings.Theme);
    }

    private readonly IEnvironment _environment;

    protected async override void OnStart()
    {
        VersionTracking.Track();

        var accountsFileServiceResolver = Startup.ServiceProvider.GetRequiredService<AccountsFileServiceResolver>();
        await accountsFileServiceResolver.Invoke().InitializeOrRefreshAccounts();

        var confirmationBase = Startup.ServiceProvider.GetRequiredService<IConfirmationService>();
        confirmationBase.Initialize();

        var updateService = Startup.ServiceProvider.GetRequiredService<IUpdateService>();
        await updateService.CheckForUpdateAndDownloadInstall(true);
    }

    protected override void OnSleep()
    {
        RequestedThemeChanged -= OnRequestedThemeChanged;
    }

    protected override void OnResume()
    {
        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        ApplyStatusBarColor();
    }

    private void ApplyStatusBarColor()
    {
        if (Application.Current.RequestedTheme == OSAppTheme.Dark)
            _environment.SetStatusBarColor((Color) Application.Current.Resources["SecondDarkBackground"], false);
        else
            _environment.SetStatusBarColor((Color) Application.Current.Resources["SecondLightBackgroundColor"], true);
    }
}