using Android.App;
using Android.Content.PM;
using SteamMobileAuthenticator.Platforms.Android.Helpers;

namespace SteamMobileAuthenticator;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        PermissionHelper.OnRequestPermissions(requestCode, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}