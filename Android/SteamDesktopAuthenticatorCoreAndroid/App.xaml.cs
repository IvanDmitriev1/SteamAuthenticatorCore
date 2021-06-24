using SteamAuthenticatorAndroid.Views;
using Xamarin.Forms;

namespace SteamAuthenticatorAndroid
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