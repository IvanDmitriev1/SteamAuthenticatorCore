using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile;

public partial class App : Application
{
    public App(AccountsServiceResolver accountsFileServiceResolver, IConfirmationService confirmationService)
    {
        InitializeComponent();

        _accountsFileServiceResolver = accountsFileServiceResolver;
        _confirmationService = confirmationService;

        MainPage = new AppShell();
        Shell.Current.Navigating += CurrentOnNavigating;
    }

    private readonly AccountsServiceResolver _accountsFileServiceResolver;
    private readonly IConfirmationService _confirmationService;

    protected override async void OnStart()
    {
        MauiAppSettings.Current.Load();

        await _accountsFileServiceResolver.Invoke().Initialize().ConfigureAwait(false);
        await _confirmationService.Initialize().ConfigureAwait(false);

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
