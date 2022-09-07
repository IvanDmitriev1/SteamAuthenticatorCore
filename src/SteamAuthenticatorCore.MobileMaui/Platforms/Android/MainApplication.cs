using Android.App;
using Android.Runtime;
using Java.Net;

[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]
[assembly: UsesPermission(Android.Manifest.Permission.RequestInstallPackages)]

namespace SteamAuthenticatorCore.MobileMaui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
