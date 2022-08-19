using System;

namespace SteamAuthenticatorCore.Shared.Models;

public class CheckForUpdateModel
{
    public CheckForUpdateModel(string appFileName, string downloadUrl, Version newVersion, bool needUpdate)
    {
        AppFileName = appFileName;
        DownloadUrl = downloadUrl;
        NewVersion = newVersion;
        NeedUpdate = needUpdate;
    }
    
    public bool NeedUpdate { get; }
    
    public string AppFileName { get; }
    
    public string DownloadUrl { get; }
    
    public Version NewVersion { get; }
}
