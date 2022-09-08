using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using SteamAuthenticatorCore.Mobile.Abstractions;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

namespace SteamAuthenticatorCore.Mobile.Services;

public class StatusBar : IStatusBar
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
        if (Application.Current!.RequestedTheme == AppTheme.Dark)
        {
            Application.Current!.Resources.TryGetValue("SecondDarkBackground", out var color);
            SetStatusBarColor((Color)color!, false);
        }
        else
        {
            Application.Current!.Resources.TryGetValue("SecondLightBackgroundColor", out var color);
            SetStatusBarColor((Color)color!, true);
        }
    }
}