using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore;
using SteamAuthCore.Manifest;
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

        var platformImplementations = Startup.ServiceProvider.GetRequiredService<IPlatformImplementations>();

        _settings = Startup.ServiceProvider.GetRequiredService<AppSettings>();
        _settings.LoadSettings();
        _settings.PropertyChanged += SettingsOnPropertyChanged;
        platformImplementations.SetTheme(_settings.AppTheme);

        var confirmationService = Startup.ServiceProvider.GetRequiredService<BaseConfirmationService>();

        var tokenService = Startup.ServiceProvider.GetRequiredService<TokenService>();
        tokenService.IsMobile = true;
    }

    private readonly AppSettings _settings;

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
        _settings.AppTheme = e.RequestedTheme switch
        {
            OSAppTheme.Unspecified => Theme.System,
            OSAppTheme.Light => Theme.Light,
            OSAppTheme.Dark => Theme.Dark,
            _ => throw new ArgumentOutOfRangeException()
        };
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