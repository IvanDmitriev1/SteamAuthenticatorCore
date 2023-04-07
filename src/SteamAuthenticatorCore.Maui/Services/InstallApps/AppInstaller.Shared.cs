namespace SteamAuthenticatorCore.Maui.Services;

public static partial class AppInstaller
{
    public static string DownloadedDirectory { get; }

    public static partial void Install(string fileName);
}