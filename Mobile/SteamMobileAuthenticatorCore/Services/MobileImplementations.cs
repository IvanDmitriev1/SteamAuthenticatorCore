using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource)
    {
        return ImageSource.FromUri(new Uri(imageSource, UriKind.Absolute));
    }

    public void InvokeMainThread(Action method)
    {
        Device.BeginInvokeOnMainThread(method);
    }

    public Task DisplayAlert(string message)
    {
        return Application.Current.MainPage.DisplayAlert("", message, "Ok");
    }
}