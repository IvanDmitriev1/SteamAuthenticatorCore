using System;
using Android.App;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.Droid;

internal class DownloadMonitor
{
    public class DownloadProgressEventArg
    {
        public DownloadProgressEventArg(string filePath, float percentage)
        {
            FilePath = filePath;
            Percentage = percentage;
        }

        public readonly string FilePath;
        public readonly float Percentage;
    }

    public DownloadStatus ComputeDownloadStatus(Int64 downloadId)
    {
        Int64 downloadedBytes = 0;
        Int64 totalSize = 0;
        int status = 0;

        var query = new DownloadManager.Query();
        query.SetFilterById(downloadId);
        var downloadManager = DownloadManager.FromContext(Android.App.Application.Context);

        var cursor = downloadManager.InvokeQuery(query);

        if (cursor == null || !cursor.MoveToFirst())
            return (DownloadStatus)status;

        downloadedBytes = cursor.GetLong(cursor.GetColumnIndexOrThrow(DownloadManager.ColumnBytesDownloadedSoFar));
        totalSize = cursor.GetInt(cursor.GetColumnIndexOrThrow(DownloadManager.ColumnTotalSizeBytes));
        status = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));

        var downloadFilePath = (cursor.GetString(cursor.GetColumnIndex(DownloadManager.ColumnLocalUri))).Replace("file://", "");

        cursor.Close();

        var percentage = (new decimal(downloadedBytes) / new decimal(totalSize)) * 100;
        NotifyDownloadProgress(downloadFilePath, (float)percentage);

        return (DownloadStatus)status;
    }

    private void NotifyDownloadProgress(string filePath, float percentage)
    {
        var args = new DownloadProgressEventArg(filePath, percentage);
        MessagingCenter.Instance.Send<object, DownloadProgressEventArg>(this, "downloading", args);
    }
}