using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Services;
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
        settings.PropertyChanged += (sender, args) =>
        {
            var senderSettings = (sender as AppSettings)!;

            senderSettings.SettingsService.SaveSetting(args.PropertyName, senderSettings);
        };
    }

    private readonly IEnvironment _environment;

    protected async override void OnStart()
    {
        var accountsWatcherService = Startup.ServiceProvider.GetRequiredService<ManifestAccountsWatcherService>();
        await accountsWatcherService.Initialize();
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