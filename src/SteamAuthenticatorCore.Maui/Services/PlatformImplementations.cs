namespace SteamAuthenticatorCore.Maui.Services;

internal class PlatformImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource)
    {
        var image = new UriImageSource
        {
            Uri = new Uri(imageSource, UriKind.Absolute),
            CachingEnabled = true,
            CacheValidity = TimeSpan.FromDays(1)
        };

        return image;
    }

    public void InvokeMainThread(Action method)
    {
        if (MainThread.IsMainThread)
        {
            method.Invoke();
            return;
        }

        MainThread.BeginInvokeOnMainThread(method);
    }

    public Task DisplayAlert(string title, string message)
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, "Ok");
    }

    public Task<bool> DisplayPrompt(string title, string message, string accept = "Ok", string cancel = "Cancel")
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
    }
}