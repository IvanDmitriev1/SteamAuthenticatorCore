namespace SteamAuthenticatorCore.Maui.MyPermissions;

public class InstallPackagesPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
    {
        (global::Android.Manifest.Permission.RequestInstallPackages, true),
    };
}