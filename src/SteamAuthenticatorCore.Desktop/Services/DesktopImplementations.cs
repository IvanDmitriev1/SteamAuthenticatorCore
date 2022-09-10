using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;
using Wpf.Ui.Controls.Interfaces;
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

    public async ValueTask InvokeMainThread(Action method)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            method.Invoke();
        else
            await Application.Current.Dispatcher.InvokeAsync(method);
    }

    public async Task DisplayAlert(string title, string message)
    {
        var control = _dialog.GetDialogControl();
        await control.ShowAndWaitAsync(title ,message, true);
    }

    public async Task<bool> DisplayPrompt(string title, string message, string accept = "Ok", string cancel = "Cancel")
    {
        var control = _dialog.GetDialogControl();
        var previousLeftButtonName = control.ButtonLeftName;
        var previousRightButtonName = control.ButtonRightName;

        control.ButtonLeftName = accept;
        control.ButtonRightName = cancel;

        var result = await control.ShowAndWaitAsync(title ,message, true);

        control.ButtonLeftName = previousLeftButtonName;
        control.ButtonRightName = previousRightButtonName;

        return result == IDialogControl.ButtonPressed.Left;
    }

    public void SetTheme(Theme theme)
    {
        
    }
}