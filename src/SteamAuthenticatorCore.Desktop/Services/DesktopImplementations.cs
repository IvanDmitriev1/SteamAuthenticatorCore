using System;
using System.Threading.Tasks;
using System.Windows;
using SteamAuthenticatorCore.Shared.Abstraction;
using System.Windows.Media.Imaging;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopImplementations : IPlatformImplementations
{
    public DesktopImplementations(IDialogService dialog)
    {
        _dialog = dialog;
    }

    private readonly IDialogService _dialog;

    public object CreateImage(string imageSource)
    {
        return new BitmapImage(new Uri(imageSource, UriKind.Absolute));
    }

    public void InvokeMainThread(Action method)
    {
        Application.Current.Dispatcher.Invoke(method);
    }

    public Task DisplayAlert(string message)
    {
        return _dialog.GetDialogControl().ShowAndWaitAsync();
    }
}