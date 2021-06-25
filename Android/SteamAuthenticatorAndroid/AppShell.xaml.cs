using SteamAuthenticatorAndroid.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorAndroid
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }
    }
}