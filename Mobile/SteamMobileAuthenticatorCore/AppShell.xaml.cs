using SteamAuthenticatorCore.Mobile.Views;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ConfirmationsPage), typeof(ConfirmationsPage));
        }
    }
}
