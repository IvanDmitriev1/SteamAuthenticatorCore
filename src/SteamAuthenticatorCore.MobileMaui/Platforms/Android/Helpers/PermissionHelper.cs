using Android.Content.PM;

namespace SteamMobileAuthenticator.Platforms.Android.Helpers;

internal static class PermissionHelper
{
    public static void  OnRequestPermissions(int requestCode, Permission[] grantResults)
    {
        if (requestCode != RequestCode)
            return;

        if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
            _completionSource.SetResult(true);
        else
            _completionSource.SetResult(false);
    }

    private static TaskCompletionSource<bool> _completionSource = new();
    private const int RequestCode = 111;

    public static Task<bool> CheckAndGrandPermission(string permission)
    {
        if (!Platform.CurrentActivity!.ShouldShowRequestPermissionRationale(permission))
        {
            return Task.FromResult(true);   
        }

        if (Platform.CurrentActivity.CheckSelfPermission(permission) == Permission.Granted)
            return Task.FromResult(true);

        if (_completionSource.Task.IsCompleted)
            _completionSource = new TaskCompletionSource<bool>();

        Platform.CurrentActivity.RequestPermissions(new []{ permission }, RequestCode);
        return _completionSource.Task;
    }
}
