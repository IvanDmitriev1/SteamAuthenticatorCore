using System;
using System.IO;
using GoogleDrive;
using Microsoft.Extensions.DependencyInjection;

namespace SteamAuthenticatorCore.Desktop;

internal static class ServiceCollectionExtension
{
    public static void AddGoogleDriveApi(this IServiceCollection services, string appName)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appdata, appName);
        var userCredentialPath = Path.Combine(appFolder, "Token.json");

        services.AddSingleton(typeof(GoogleDriveApi),
            new GoogleDriveApi(userCredentialPath, new[] { Google.Apis.Drive.v3.DriveService.Scope.DriveFile },
                appName));
    }
}