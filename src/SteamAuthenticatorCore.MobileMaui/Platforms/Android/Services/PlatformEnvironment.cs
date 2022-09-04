using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using SteamAuthenticatorCore.MobileMaui.Abstractions;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

namespace SteamAuthenticatorCore.MobileMaui.Services;

public class PlatformEnvironment : IEnvironment
{
    public void SetStatusBarColor(Color color, bool darkStatusBarTint)
    {
        var activity = Platform.CurrentActivity!;
        var window = activity.Window!;

        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        window.ClearFlags(WindowManagerFlags.TranslucentStatus);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            WindowCompat.GetInsetsController(window, window.DecorView).AppearanceLightStatusBars = darkStatusBarTint;
        }

        window.SetStatusBarColor(color.ToAndroid());
    }

    public void SetStatusBarColorBasedOnAppTheme()
    {
        var dictionary = Application.Current!.Resources.MergedDictionaries.ElementAt(0);

        if (Application.Current.RequestedTheme == AppTheme.Dark)
            SetStatusBarColor((Color)dictionary["SecondDarkBackground"], false);
        else
            SetStatusBarColor((Color)dictionary["SecondLightBackgroundColor"], true);
    }
}