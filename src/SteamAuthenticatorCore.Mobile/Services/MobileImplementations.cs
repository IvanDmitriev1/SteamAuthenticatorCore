using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstraction;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource)
    {
        return ImageSource.FromUri(new Uri(imageSource, UriKind.Absolute));
    }

    public async ValueTask InvokeMainThread(Action method)
    {
        await Device.InvokeOnMainThreadAsync(method);
    }

    public Task DisplayAlert(string message)
    {
        return Application.Current.MainPage.DisplayAlert("Alert", message, "Ok");
    }
}