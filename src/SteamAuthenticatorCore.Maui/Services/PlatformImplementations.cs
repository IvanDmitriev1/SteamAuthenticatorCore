using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.Services;

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

    public async ValueTask InvokeMainThread(Action method)
    {
        if (MainThread.IsMainThread)
        {
            method.Invoke();
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(method);
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