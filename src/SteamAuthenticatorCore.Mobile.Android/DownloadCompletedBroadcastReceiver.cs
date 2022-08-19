using System;
using Android.App;
using Android.Content;

namespace SteamMobileAuthenticatorCore.Droid;

[BroadcastReceiver(Exported = false)]
[IntentFilter(new string[] { DownloadManager.ActionDownloadComplete })]
public class DownloadCompletedBroadcastReceiver : Android.Content.BroadcastReceiver
{
    public Action<string>? OnSuccessful { get; set; }
    public Action<int>? OnFailed { get; set; }

    public override void OnReceive(Context? context, Intent? intent)
    {
        var action = intent.Action;
        if (!DownloadManager.ActionDownloadComplete.Equals(action) || intent.Extras == null)
            return;

        var extras = intent.Extras;
        var query = new DownloadManager.Query();
        var downloadId = extras.GetLong(DownloadManager.ExtraDownloadId);
        query.SetFilterById(downloadId);
        var cursor = ((DownloadManager)context.GetSystemService(Context.DownloadService)).InvokeQuery(query);

        if (cursor == null || !cursor.MoveToFirst())
            return;

        int status = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));
        var downloadFilePath = (cursor.GetString(cursor.GetColumnIndex(DownloadManager.ColumnLocalUri))).Replace("file://", "");
        var downloadTitle = cursor.GetString(cursor.GetColumnIndex(DownloadManager.ColumnTitle));

        if (status == (int)DownloadStatus.Successful)
        {
            OnSuccessful?.Invoke(downloadFilePath);
        }
        else if (status == (int)DownloadStatus.Failed)
        {
            var code = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnReason));
            OnFailed?.Invoke(code);
        }

        cursor.Close();
    }
}