using System;
using SteamMobileAuthenticatorCore.Services;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore
{
    public partial class App : Application
    {
        public static IManifestModelService ManifestModelService { get; private set; } = null!;

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            ManifestModelService = new LocalDriveManifestModelService(new MobileDirectoryService(), new MobileManifestAdditionalSettingsService());
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {
        }
    }
}
