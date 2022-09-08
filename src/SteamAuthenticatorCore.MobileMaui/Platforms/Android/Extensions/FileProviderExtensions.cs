using FileProvider = AndroidX.Core.Content.FileProvider;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace SteamMobileAuthenticator.Platforms.Android.Extensions;

internal static class FileProviderExtensions
{
    public static Uri? GetUriForFile(string filePath) => FileProvider.GetUriForFile(Platform.AppContext, $"{Platform.AppContext.PackageName}.fileProvider", new File(filePath));
}
