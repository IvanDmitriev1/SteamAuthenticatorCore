#nullable enable

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Java.IO;
using Sentry;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
using FileProvider = AndroidX.Core.Content.FileProvider;
using Uri = Android.Net.Uri;

namespace SteamMobileAuthenticatorCore.Droid;

internal class AndroidUpdateService : UpdateServiceBase
    {
        public AndroidUpdateService(HttpClient client) : base(client)
        {
        }

        private readonly DownloadMonitor _downloadMonitor = new();
        private DownloadCompletedBroadcastReceiver? _receiver;

        public async override ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground)
        {
            using var dialogBuilder = new AlertDialog.Builder(Platform.CurrentActivity);
            using var alert = new AlertDialogHelper(dialogBuilder, "Update");

            CheckForUpdateModel? model;
            try
            {
                model = await CheckForUpdate($"{Platform.AppContext.PackageName}.apk", new Version(VersionTracking.CurrentVersion));
                if (model is null)
                {
                    if (!isInBackground)
                    {
                        await alert.ShowAndWait("Failed to fetch update", "ok");
                    }

                    return;
                }

            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);

                await alert.ShowAndWait("Failed to fetch update", "ok");
                return;
            }

            if (!model.NeedUpdate)
            {
                if (!isInBackground)
                {
                    await alert.ShowAndWait("You are using the latest version", "ok");
                }

                return;
            }

            var result = await alert.ShowAndWait("A new version available\n" + "download and install now?", "Yes", "Later");

            if (result != AlertDialogHelper.ButtonPressed.Button1)
                return;

            try
            {
                await DownloadAndInstall(new CheckForUpdateModel(model.AppFileName, model.DownloadUrl, new Version(0, 0, 0), false));
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                return;
            }
        }

        public async override Task DownloadAndInstall(CheckForUpdateModel updateModel)
        {
            await MainActivity.CheckOrGrandPermission<Permissions.StorageWrite>();
            await MainActivity.CheckOrGrandPermission<Permissions.StorageRead>();

            _receiver = new DownloadCompletedBroadcastReceiver();
            _receiver.OnSuccessful = OnSuccessful;
            _receiver.OnFailed += OnFailed;
            Application.Context.RegisterReceiver(_receiver, new IntentFilter(DownloadManager.ActionDownloadComplete));

            var manager = DownloadManager.FromContext(Application.Context)!;
            var request = new DownloadManager.Request(Uri.Parse(updateModel.DownloadUrl));

            request.SetNotificationVisibility(DownloadVisibility.Visible);
            request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, updateModel.AppFileName);
            request.SetTitle("Downloading SteamAuthenticatorCore.Mobile.Android");
            request.SetDescription(updateModel.AppFileName);

            var downloadId = manager.Enqueue(request);
            MonitorDownload(downloadId);
        }

        private void MonitorDownload(Int64 downloadId)
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                try
                {
                    var downloadStatus = _downloadMonitor.ComputeDownloadStatus(downloadId);

                    return downloadStatus != DownloadStatus.Failed && downloadStatus != DownloadStatus.Successful;
                }
                catch (Exception e)
                {
                    SentrySdk.CaptureException(e);
                    return false;
                }
            });
        }

        private void OnSuccessful(string filePath)
        {
            if (_receiver != null)
            {
                _receiver.OnSuccessful = null;
                _receiver.OnFailed = null;
                Application.Context.UnregisterReceiver(_receiver);
            }

            var uri = FileProvider.GetUriForFile(Application.Context, $"{Application.Context.PackageName}.provider", new File(filePath));

            var intent = new Intent(Intent.ActionView);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            intent.AddFlags(ActivityFlags.NewTask);
            intent.SetDataAndType(uri, "application/vnd.android.package-archive");

            Application.Context.StartActivity(intent);
        }

        private void OnFailed(int obj)
        {
            if (_receiver != null)
            {
                _receiver.OnSuccessful = null;
                _receiver.OnFailed = null;
                Application.Context.UnregisterReceiver(_receiver);
            }


        }
    }