using SteamAuthenticatorCore.Mobile.Helpers;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile;

public partial class App : Application
{
    public App(AccountsFileServiceResolver accountsFileServiceResolver, IUpdateService updateService, IConfirmationService confirmationService)
    {
        InitializeComponent();
        
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _updateService = updateService;
        _confirmationService = confirmationService;

        MainPage = new AppShell();
        Shell.Current.Navigating += CurrentOnNavigating;
    }

    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly IUpdateService _updateService;
    private readonly IConfirmationService _confirmationService;

    protected override async void OnStart()
    {
        VersionTracking.Track();
        AppSettings.Current.Load();

        ColorsCollection.Add("SecondBackgroundSelectionColor", "SecondLightBackgroundSelectionColor", "SecondDarkBackgroundSelectionColor");
        ColorsCollection.Add("SecondBackgroundColor", "SecondLightBackgroundColor", "SecondDarkBackground");

        await _accountsFileServiceResolver.Invoke().InitializeOrRefreshAccounts().ConfigureAwait(false);
        await _updateService.CheckForUpdateAndDownloadInstall(true).ConfigureAwait(false);
        _confirmationService.Initialize();

        OnResume();
    }

    protected override void OnResume()
    {
        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    protected override void OnSleep()
    {
        RequestedThemeChanged -= OnRequestedThemeChanged;
    }

    private static void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        MauiAppSettings.ChangeStatusBar(e.RequestedTheme);
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
