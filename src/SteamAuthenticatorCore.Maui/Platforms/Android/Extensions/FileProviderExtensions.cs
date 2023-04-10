using FileProvider = AndroidX.Core.Content.FileProvider;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace SteamAuthenticatorCore.Maui.Platforms.Android.Extensions;

internal static class FileProviderExtensions
{
    public static Uri? GetUriForFile(File file) => FileProvider.GetUriForFile(Platform.AppContext, $"{Platform.AppContext.PackageName}.fileProvider", file);
}
