using SteamMobileAuthenticatorCore.Services;
using SteamAuthCore;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore
{
    public partial class App : Application
    {
        public static IManifestModelService ManifestModelService { get; private set; }

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            ManifestModelService = new LocalDriveManifestModelService(new MobileDirectoryService());
        }

        protected override void OnStart()
        {
            //await ManifestModelService.Initialize();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
