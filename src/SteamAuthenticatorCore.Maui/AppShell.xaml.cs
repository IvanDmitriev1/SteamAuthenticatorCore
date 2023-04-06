namespace SteamAuthenticatorCore.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(AccountConfirmationsPage), typeof(AccountConfirmationsPage));
    }
}
