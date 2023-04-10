using Android.Content;
using SteamAuthenticatorCore.Maui.Platforms.Android.Extensions;
using File = Java.IO.File;

namespace SteamAuthenticatorCore.Maui.Services;

public static partial class AppInstaller
{
    static AppInstaller()
    {
        var file = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
        DownloadedDirectory = file!.Path;
    }

    public static partial void Install(string fileName)
    {
        var file = new File(DownloadedDirectory, fileName);
        var uri = FileProviderExtensions.GetUriForFile(file);

        var intent = new Intent(Intent.ActionView);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
        intent.AddFlags(ActivityFlags.NewTask);
        intent.SetDataAndType(uri, "application/vnd.android.package-archive");

        Platform.AppContext.StartActivity(intent);
    }
}