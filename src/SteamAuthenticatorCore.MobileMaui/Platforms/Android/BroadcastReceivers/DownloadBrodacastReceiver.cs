using Android.App;
using Android.Content;
using CommunityToolkit.Maui.Alerts;

namespace SteamAuthenticatorCore.MobileMaui.Platforms.Android.BroadcastReceivers;

[BroadcastReceiver(Exported = false)]
[IntentFilter(new string[] { DownloadManager.ActionDownloadComplete })]
public class DownloadCompleteBroadcastReceiver : BroadcastReceiver
{
    public DownloadCompleteBroadcastReceiver()
    {
        
    }

    public void RegisterOnSuccessfulCallBack(Action<string> onSuccessful)
    {
        _onSuccessful = onSuccessful;
        Platform.AppContext.RegisterReceiver(this, new IntentFilter(DownloadManager.ActionDownloadComplete));
    }

    private Action<string> _onSuccessful = null!;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent is null || context is null)
            return;

        if (!DownloadManager.ActionDownloadComplete.Equals(intent.Action) || intent.Extras == null)
            return;

        var query = new DownloadManager.Query();
        var downloadId = intent.Extras.GetLong(DownloadManager.ExtraDownloadId);
        query.SetFilterById(downloadId);

        var cursor = ((DownloadManager)context.GetSystemService(Context.DownloadService)!)?.InvokeQuery(query);

        if (cursor == null || !cursor.MoveToFirst())
            return;

        var status = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));
        var downloadFilePath =
            cursor.GetString(cursor.GetColumnIndex(DownloadManager.ColumnLocalUri))!.Replace("file://", "");

        if (status == (int) DownloadStatus.Successful)
        {
            _onSuccessful.Invoke(downloadFilePath);

            Platform.AppContext.UnregisterReceiver(this);
            Dispose();
        }
        else if (status == (int) DownloadStatus.Failed)
        {
            var code = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnReason));
            Toast.Make($"Download filed: {code}").Show();

            Platform.AppContext.UnregisterReceiver(this);
            Dispose();
        }

        cursor.Close();
    }
}
