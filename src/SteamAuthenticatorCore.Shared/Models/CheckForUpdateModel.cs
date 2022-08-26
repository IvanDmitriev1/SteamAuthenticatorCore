using System;

namespace SteamAuthenticatorCore.Shared.Models;

public class CheckForUpdateModel
{
    public CheckForUpdateModel(string downloadUrl, Version newVersion, bool needUpdate)
    {
        DownloadUrl = downloadUrl;
        NewVersion = newVersion;
        NeedUpdate = needUpdate;
    }
    
    public bool NeedUpdate { get; }
    public string DownloadUrl { get; }
    public Version NewVersion { get; }
}
