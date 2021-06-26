using SteamAuthenticatorAndroid.Services;
using Xamarin.Forms;

namespace SteamAuthenticatorAndroid
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnSleep()
        {
            await ManifestModelService.SaveManifest();
            
            base.OnSleep();
        }
    }
}