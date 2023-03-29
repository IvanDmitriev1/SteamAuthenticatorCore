using Android.App;
using Android.Content.PM;
using SteamAuthenticatorCore.Mobile.Platforms.Android.Helpers;

[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]
[assembly: UsesPermission(Android.Manifest.Permission.RequestInstallPackages)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage)]

namespace SteamAuthenticatorCore.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        PermissionHelper.OnRequestPermissions(requestCode, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}