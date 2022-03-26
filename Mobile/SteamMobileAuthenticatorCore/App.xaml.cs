using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Services;
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

        DependencyService.Register<IPlatformTimer, MobileTimer>();
        DependencyService.Register<IManifestModelService, LocalDriveManifestModelService>();
        DependencyService.Register<ObservableCollection<SteamGuardAccount>>();
        DependencyService.Register<IPlatformImplementations, MobileImplementations>();

        DependencyService.RegisterSingleton(new AppSettings(new MobileSettingsService()));
        DependencyService.RegisterSingleton(new TokenService(DependencyService.Get<IPlatformTimer>(DependencyFetchTarget.NewInstance)));
        DependencyService.RegisterSingleton( (BaseConfirmationService) new MobileConfirmationService(DependencyService.Get<ObservableCollection<SteamGuardAccount>>(), DependencyService.Get<AppSettings>(), DependencyService.Get<IPlatformImplementations>(), DependencyService.Get<IPlatformTimer>(DependencyFetchTarget.NewInstance)));
        DependencyService.RegisterSingleton(new LoginService(DependencyService.Get<IPlatformImplementations>()));


        _platformImplementations = DependencyService.Get<IPlatformImplementations>();

        _settings = DependencyService.Get<AppSettings>();
        _settings.LoadSettings();
        _settings.PropertyChanged += SettingsOnPropertyChanged;
        _platformImplementations.SetTheme(_settings.AppTheme);

        var confirmationService = DependencyService.Get<BaseConfirmationService>();

        var tokenService = DependencyService.Get<TokenService>();
        tokenService.IsMobile = true;
    }

    private readonly IPlatformImplementations _platformImplementations;
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
        var manifestService = DependencyService.Get<IManifestModelService>();

        await manifestService.Initialize(new MobileDirectoryService());
        await RefreshAccounts(manifestService);
    }

    private static async Task RefreshAccounts(IManifestModelService manifestModelService)
    {
        var accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
        accounts.Clear();

        foreach (var account in await manifestModelService.GetAccounts())
            accounts.Add(account);
    }

    private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var settings = (sender as AppSettings)!;

        settings.SettingsService.SaveSetting(e.PropertyName, settings);

        if (e.PropertyName != nameof(settings.AppTheme)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _platformImplementations.SetTheme(settings.AppTheme);
        });
    }
}