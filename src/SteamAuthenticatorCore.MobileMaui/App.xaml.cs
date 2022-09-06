using SteamAuthenticatorCore.MobileMaui.Abstractions;
using SteamAuthenticatorCore.MobileMaui.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.MobileMaui;

public partial class App : Application
{
    public App(IEnvironment environment, AppSettings appSettings, IPlatformImplementations platformImplementations, AccountsFileServiceResolver accountsFileServiceResolver, IUpdateService updateService)
    {
        InitializeComponent();

        _environment = environment;
        _appSettings = appSettings;
        _platformImplementations = platformImplementations;
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _updateService = updateService;

        MainPage = new AppShell();
        Shell.Current.Navigating += CurrentOnNavigating;
    }

    private readonly IEnvironment _environment;
    private readonly AppSettings _appSettings;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly IUpdateService _updateService;

    protected async override void OnStart()
    {
        VersionTracking.Track();

        _appSettings.LoadSettings();
        _platformImplementations.SetTheme(_appSettings.Theme);

        await _accountsFileServiceResolver.Invoke().InitializeOrRefreshAccounts().ConfigureAwait(false);
        await _updateService.CheckForUpdateAndDownloadInstall(true).ConfigureAwait(false);

        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    protected override void OnSleep()
    {
        RequestedThemeChanged -= OnRequestedThemeChanged;
    }

    protected override void OnResume()
    {
        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        _environment.SetStatusBarColorBasedOnAppTheme();
    }

    private static void CurrentOnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (e.Target.Location.OriginalString != string.Empty)
            return;

        if (e.Current.Location.OriginalString.Contains(nameof(TokenPage)))
            return;

        e.Cancel();

        var shell = (Shell)sender!;
        shell.GoToAsync($"//{nameof(TokenPage)}");
    }
}
