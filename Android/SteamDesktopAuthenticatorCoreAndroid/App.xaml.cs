using SteamDesktopAuthenticatorCoreAndroid.Views;
using Xamarin.Forms;

namespace SteamDesktopAuthenticatorCoreAndroid
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}