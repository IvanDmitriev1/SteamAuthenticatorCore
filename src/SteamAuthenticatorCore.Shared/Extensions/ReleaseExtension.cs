using System.IO;
using Octokit;
using System.Linq;

namespace SteamAuthenticatorCore.Shared.Extensions;

public static class ReleaseExtension
{
    public static ReleaseAsset[] FindSuitableAssets(this Release release, string fileExtension)
    {
        return release.Assets.Where(asset => Path.GetExtension(asset.BrowserDownloadUrl) == fileExtension).ToArray();
    }
}