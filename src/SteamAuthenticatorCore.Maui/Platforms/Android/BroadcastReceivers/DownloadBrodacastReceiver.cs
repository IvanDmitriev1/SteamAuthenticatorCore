using Android.App;
using Android.Content;
using Microsoft.Extensions.Logging;

namespace SteamAuthenticatorCore.Maui.Platforms.Android.BroadcastReceivers;

[BroadcastReceiver(Exported = false)]
[IntentFilter(new string[] { DownloadManager.ActionDownloadComplete })]
public class DownloadCompleteBroadcastReceiver : BroadcastReceiver
{
    public DownloadCompleteBroadcastReceiver()
    {
        
    }

    public void RegisterOnSuccessfulCallBack(ILogger<DownloadCompleteBroadcastReceiver> logger, Action<string> onSuccessful)
    {
        _logger = logger;
        _onSuccessful = onSuccessful;
        Platform.AppContext.RegisterReceiver(this, new IntentFilter(DownloadManager.ActionDownloadComplete));
    }

    private Action<string> _onSuccessful = null!;
    private ILogger<DownloadCompleteBroadcastReceiver> _logger = null!;

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
            try
            {
                _onSuccessful.Invoke(downloadFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when invoking  _onSuccessful action");
            }
            finally
            {
                Platform.AppContext.UnregisterReceiver(this);
                Dispose();
            }
        }
        else if (status == (int) DownloadStatus.Failed)
        {
            var code = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnReason));
            Toast.Make($"Download filed: {code}").Show().ConfigureAwait(false);

            Platform.AppContext.UnregisterReceiver(this);
            Dispose();
        }

        cursor.Close();
    }
}
