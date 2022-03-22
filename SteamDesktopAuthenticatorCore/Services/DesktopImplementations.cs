using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SteamAuthenticatorCore.Shared;
using WPFUI.DIControls.Interfaces;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopImplementations : IPlatformImplementations
{
    public DesktopImplementations(IDialog dialog)
    {
        _dialog = dialog;
    }

    private readonly IDialog _dialog;

    public object CreateImage(string imageSource)
    {
        return new BitmapImage(new Uri(imageSource, UriKind.Absolute));
    }

    public void InvokeMainThread(Action method)
    {
        Application.Current.Dispatcher.Invoke(method.Invoke);
    }

    public Task DisplayAlert(string message)
    {
        return _dialog.ShowDialog(message);
    }
}