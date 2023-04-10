using SteamAuthenticatorCore.Maui.Pages;

namespace SteamAuthenticatorCore.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(AccountConfirmationsPage), typeof(AccountConfirmationsPage));
        Routing.RegisterRoute(nameof(AvailableLanguagesPage), typeof(AvailableLanguagesPage));
    }
}
