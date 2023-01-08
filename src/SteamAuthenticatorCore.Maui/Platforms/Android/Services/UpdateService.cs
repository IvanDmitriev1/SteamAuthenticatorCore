using Android;
using Android.App;
using Android.Content;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using SteamAuthenticatorCore.Mobile.Platforms.Android.BroadcastReceivers;
using SteamAuthenticatorCore.Mobile.Platforms.Android.Extensions;
using SteamAuthenticatorCore.Mobile.Platforms.Android.Helpers;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using Application = Microsoft.Maui.Controls.Application;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace SteamAuthenticatorCore.Mobile.Services;

public class UpdateService : UpdateServiceBase
{
    public UpdateService(HttpClient client, ILoggerFactory loggerFactory) : base(client)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<UpdateService>();
    }

    private readonly ILogger<UpdateService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private static readonly string FileName = $"{Platform.AppContext.PackageName}.apk";
    private const string DownloadedFileKey = "DownloadedFilePath";
    private const string CurrentVersionKey = "CurrentVersion";

    public async override ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground)
    {
        CheckForUpdateModel? model;
        try
        {
            model = await CheckForUpdate("apk", new Version(VersionTracking.CurrentVersion));
            if (model is null)
            {
                if (!isInBackground)
                    await Application.Current!.MainPage!.DisplayAlert("Updater", "Failed to fetch update", "ok");

                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch update");
            await Application.Current!.MainPage!.DisplayAlert("Updater", "Failed to fetch update", "ok");
            return;
        }

        if (!model.NeedUpdate)
        {
            if (!isInBackground)
                await Application.Current!.MainPage!.DisplayAlert("Updater", "You are using the latest version", "ok");

            return;
        }

        var result = await Application.Current!.MainPage!.DisplayAlert("Updater", "A new version available\n" + "download and install now?", "Yes", "Later");
        if (!result)
            return;

        try
        {
            await DownloadAndInstall(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start downloading");
        }
    }

    public async override Task DownloadAndInstall(CheckForUpdateModel updateModel)
    {
        var request1 = await PermissionHelper.CheckAndGrandPermission(Manifest.Permission.InstallPackages);
        var request2 = await PermissionHelper.CheckAndGrandPermission(Manifest.Permission.RequestInstallPackages);

        if (!request1 || !request2)
        {
            await Application.Current!.MainPage!.DisplaySnackbar("One or more permissions were denied");
            return;
        }

        if (CheckIfFileIsDownload())
            return;

        var broadcastReceiver = new DownloadCompleteBroadcastReceiver();
        broadcastReceiver.RegisterOnSuccessfulCallBack(_loggerFactory.CreateLogger<DownloadCompleteBroadcastReceiver>(), OnSuccessful);

        var manager = DownloadManager.FromContext(Platform.AppContext)!;
        var request = new DownloadManager.Request(Uri.Parse(updateModel.DownloadUrl));

        request.SetNotificationVisibility(DownloadVisibility.Visible);
        request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, FileName);
        request.SetTitle("Downloading SteamAuthenticatorCore.Mobile.Android");
        request.SetDescription(FileName);

        var downloadId = manager.Enqueue(request);
        await Toast.Make($"Download started").Show();
    }

    private static void OnSuccessful(string filePath)
    {
        Preferences.Set(DownloadedFileKey, filePath);
        Preferences.Set(CurrentVersionKey, VersionTracking.CurrentVersion);

        var uri = FileProviderExtensions.GetUriForFile(filePath);
        StartInstalling(uri!);
    }

    private static bool CheckIfFileIsDownload()
    {
        var currentVersion = Preferences.Get(CurrentVersionKey, null);
        if (currentVersion is null || currentVersion != VersionTracking.CurrentVersion)
            return false;

        var filePath = Preferences.Get(DownloadedFileKey, null);
        if (filePath is null)
            return false;

        var uri = FileProviderExtensions.GetUriForFile(filePath);
        if (uri is null)
            return false;

        StartInstalling(uri);
        return true;
    }

    private static void StartInstalling(Uri uri)
    {
        var intent = new Intent(Intent.ActionView);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
        intent.AddFlags(ActivityFlags.NewTask);
        intent.SetDataAndType(uri, "application/vnd.android.package-archive");

        Platform.AppContext.StartActivity(intent);
    }
}
