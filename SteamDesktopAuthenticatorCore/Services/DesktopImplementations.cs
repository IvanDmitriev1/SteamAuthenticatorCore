using System;
using System.Windows;
using System.Windows.Media.Imaging;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource)
    {
        return new BitmapImage(new Uri(imageSource, UriKind.Absolute));
    }

    public void InvokeMainThread(Action method)
    {
        Application.Current.Dispatcher.Invoke(method.Invoke);
    }
}