#nullable enable
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using SteamAuthenticatorCore.Mobile;
using SteamAuthenticatorCore.Mobile.Helpers;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;
using SteamAuthenticatorCore.Shared.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.Droid
{
    [Activity(Label = "SteamMobileAuthenticatorCore", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, Exported = true)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SentryXamarin.Init(options =>
            {
                options.Dsn = "https://4a0459ed781e49c28e6c8e85da244344@o1354225.ingest.sentry.io/6658167";
                options.AddXamarinFormsIntegration();
                options.AttachScreenshots = true;
            });

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            Startup.Init(NativeConfiguration);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async override void OnBackPressed()
        {
            if (Shell.Current.CurrentPage is not IBackButtonAction backButtonAction)
            {
                base.OnBackPressed();
                return;
            }

            if (backButtonAction.OnBackActionAsync is null)
                return;

            if (!await backButtonAction.OnBackActionAsync.Invoke()) 
                base.OnBackPressed();
        }

        public static async ValueTask CheckOrGrandPermission<TPermission>() where TPermission : Permissions.BasePermission, new()
        {
            if (await Permissions.CheckStatusAsync<TPermission>() == PermissionStatus.Granted)
                return;

            await Permissions.RequestAsync<TPermission>();
        }

        private static void NativeConfiguration(IServiceCollection services)
        {
            services.AddSingleton<IEnvironment, AndroidEnvironment>();
            services.AddScoped<IUpdateService, AndroidUpdateService>();

            services.AddHttpClient<IUpdateService, AndroidUpdateService>();
        }
    }
}