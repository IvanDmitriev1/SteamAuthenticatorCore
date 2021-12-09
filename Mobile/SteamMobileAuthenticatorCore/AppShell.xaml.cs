using SteamMobileAuthenticatorCore.Views;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }
    }
}
