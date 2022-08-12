using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;
using SteamAuthenticatorCore.Shared;
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

        var platformImplementations = Startup.ServiceProvider.GetRequiredService<IPlatformImplementations>();

        var settings = Startup.ServiceProvider.GetRequiredService<AppSettings>();
        settings.LoadSettings();
        settings.PropertyChanged += SettingsOnPropertyChanged;
        platformImplementations.SetTheme(settings.AppTheme);
        ApplyStatusBarColor();

        var confirmationService = Startup.ServiceProvider.GetRequiredService<BaseConfirmationService>();

        var tokenService = Startup.ServiceProvider.GetRequiredService<TokenService>();
        tokenService.IsMobile = true;
    }

    private readonly IEnvironment _environment;

    protected override async void OnStart()
    {
        await OnManifestLocationChanged();

        OnResume();
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

    private static async Task OnManifestLocationChanged()
    {
        var manifestService = Startup.ServiceProvider.GetRequiredService<IManifestModelService>();

        await manifestService.Initialize();
        await RefreshAccounts(manifestService);
    }

    private static async Task RefreshAccounts(IManifestModelService manifestModelService)
    {
        var accounts = Startup.ServiceProvider.GetRequiredService<ObservableCollection<SteamGuardAccount>>();
        accounts.Clear();

        foreach (var account in await manifestModelService.GetAccounts())
            accounts.Add(account);
    }

    private static void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var settings = (sender as AppSettings)!;

        settings.SettingsService.SaveSetting(e.PropertyName, settings);

        if (e.PropertyName != nameof(settings.AppTheme)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var platformImplementations = Startup.ServiceProvider.GetRequiredService<IPlatformImplementations>();
            platformImplementations.SetTheme(settings.AppTheme);
        });
    }
}