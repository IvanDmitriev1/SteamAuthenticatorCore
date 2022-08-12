using System.Drawing;
using Android.OS;
using AndroidX.Core.View;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;
using Xamarin.Essentials;

namespace SteamMobileAuthenticatorCore.Droid;

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