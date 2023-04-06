using Android.App;
using Android.Content.PM;

[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]
[assembly: UsesPermission(Android.Manifest.Permission.RequestInstallPackages)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage)]

namespace SteamAuthenticatorCore.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    
}