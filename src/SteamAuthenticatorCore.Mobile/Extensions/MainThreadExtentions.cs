namespace SteamAuthenticatorCore.Mobile.Extensions;

internal static class MainThreadExtensions
{
    public static async ValueTask InvokeOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action.Invoke();
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(action);
    }
}
