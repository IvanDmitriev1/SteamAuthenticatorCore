using SteamAuthenticatorCore.MobileMaui.Pages;

namespace SteamAuthenticatorCore.MobileMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }
}
