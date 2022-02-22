using System;
using SteamAuthenticatorCore.Shared;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource)
    {
        return new Image()
        {
            Aspect = Aspect.AspectFit,
            Source = ImageSource.FromUri(new Uri(imageSource, UriKind.Absolute))
        };
    }

    public void InvokeMainThread(Action method)
    {
        Device.BeginInvokeOnMainThread(method);
    }
}