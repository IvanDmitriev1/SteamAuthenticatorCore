using SteamAuthenticatorCore.MobileMaui.Abstractions;
using SteamAuthenticatorCore.MobileMaui.Pages;

namespace SteamAuthenticatorCore.MobileMaui;

public partial class App : Application
{
    public App(IEnvironment environment)
    {
        _environment = environment;
        InitializeComponent();

        MainPage = new AppShell();

        Shell.Current.Navigating += CurrentOnNavigating;
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

    private readonly IEnvironment _environment;

    protected override void OnStart()
    {
        VersionTracking.Track();
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
        ApplyStatusBarColor();
    }

    private void ApplyStatusBarColor()
    {
        var dictionary = Current!.Resources.MergedDictionaries.ElementAt(0);

        if (Current.RequestedTheme == AppTheme.Dark)
            _environment.SetStatusBarColor((Color) dictionary["SecondDarkBackground"], false);
        else 
            _environment.SetStatusBarColor((Color) dictionary["SecondLightBackgroundColor"], true);
    }
}
