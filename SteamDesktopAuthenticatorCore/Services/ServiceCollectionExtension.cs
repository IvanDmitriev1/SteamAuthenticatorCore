using System;
using System.IO;
using GoogleDrive;
using Microsoft.Extensions.DependencyInjection;

namespace SteamDesktopAuthenticatorCore.Services
{
    internal static class ServiceCollectionExtension
    {
        public static void AddGoogleDriveApi(this IServiceCollection services, string appName)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appdata, appName);
            string userCredentialPath = Path.Combine(appFolder, "Token.json");

            services.AddSingleton(typeof(GoogleDriveApi),
                new GoogleDriveApi(userCredentialPath, new[] { Google.Apis.Drive.v3.DriveService.Scope.DriveFile },
                    appName));
        }
    }
}
