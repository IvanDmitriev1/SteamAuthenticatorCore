using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Helpers;
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

        Theme.SetTheme();
        MainPage = new AppShell();

        DependencyService.Register<IPlatformTimer, MobileTimer>();
        DependencyService.Register<IManifestModelService, LocalDriveManifestModelService>();
        DependencyService.Register<ObservableCollection<SteamGuardAccount>>();
        DependencyService.Register<IPlatformImplementations, MobileImplementations>();

        DependencyService.RegisterSingleton(new AppSettings(new MobileSettingsService()));
        DependencyService.RegisterSingleton(new TokenService(DependencyService.Get<IPlatformTimer>(DependencyFetchTarget.NewInstance)));
        DependencyService.RegisterSingleton( (BaseConfirmationService) new MobileConfirmationService(DependencyService.Get<ObservableCollection<SteamGuardAccount>>(), DependencyService.Get<AppSettings>(), DependencyService.Get<IPlatformImplementations>(), DependencyService.Get<IPlatformTimer>(DependencyFetchTarget.NewInstance)));
        DependencyService.RegisterSingleton(new LoginService(DependencyService.Get<IPlatformImplementations>()));

        var confirmationService = DependencyService.Get<BaseConfirmationService>();

        var tokenService = DependencyService.Get<TokenService>();
        tokenService.IsMobile = true;
    }

    protected override async void OnStart()
    {
        await OnLoadingAsync();

        OnResume();
    }

    protected override async void OnSleep()
    {
        Theme.SetTheme();
        RequestedThemeChanged -= OnRequestedThemeChanged;

        await DependencyService.Get<AppSettings>().SaveSettingsAsync();
    }

    protected override void OnResume()
    {
        Theme.SetTheme();
        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    private static void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(Theme.SetTheme);
    }

    private static async Task OnLoadingAsync()
    {
        var settings = DependencyService.Get<AppSettings>();
        await settings.LoadSettingsAsync();
        

        await OnManifestLocationChanged();
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
}