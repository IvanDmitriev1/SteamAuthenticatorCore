using Android.Content;
using SteamAuthenticatorCore.Maui.Platforms.Android.Extensions;


namespace SteamAuthenticatorCore.Maui.Services;

public static partial class AppInstaller
{
    public static partial void Install(string filePath)
    {
        var uri = FileProviderExtensions.GetUriForFile(filePath);
        
        var intent = new Intent(Intent.ActionView);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
        intent.AddFlags(ActivityFlags.NewTask);
        intent.SetDataAndType(uri, "application/vnd.android.package-archive");

        Platform.AppContext.StartActivity(intent);
    }
}