using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.Core.View;
using SteamAuthenticatorCore.Mobile;
using SteamAuthenticatorCore.Mobile.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Color = System.Drawing.Color;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;

[assembly: Dependency(typeof(SteamMobileAuthenticatorCore.Droid.AndroidEnvironment))]
namespace SteamMobileAuthenticatorCore.Droid
{
    [Activity(Label = "SteamMobileAuthenticatorCore", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override async void OnBackPressed()
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
    }

    public class AndroidEnvironment : IEnvironment
    {
        public void SetStatusBarColor(Color color, bool darkStatusBarTint)
        {
            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
                return;

            var activity = Platform.CurrentActivity;
            var window = activity.Window!;

            //this may not be necessary(but may be fore older than M)
            window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                WindowCompat.GetInsetsController(window, window.DecorView).AppearanceLightStatusBars = darkStatusBarTint;
            }

            window.SetStatusBarColor(color.ToPlatformColor());
        }
    }
}