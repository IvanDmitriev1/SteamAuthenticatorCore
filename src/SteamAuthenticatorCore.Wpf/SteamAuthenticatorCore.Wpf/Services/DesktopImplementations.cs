using System.Windows.Media.Imaging;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource)
    {
        var image = new BitmapImage(new Uri(imageSource, UriKind.Absolute))
        {
            CacheOption = BitmapCacheOption.OnDemand,
            CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.PreservePixelFormat,
        };

        return image;
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
        /*var control = _dialog.GetDialogControl();
        await control.ShowAndWaitAsync(title ,message);

        control.Hide();*/
    }

    public async Task<bool> DisplayPrompt(string title, string message, string accept = "Ok", string cancel = "Cancel")
    {
        /*var control = _dialog.GetDialogControl();
        var previousLeftButtonName = control.ButtonLeftName;
        var previousRightButtonName = control.ButtonRightName;

        control.ButtonLeftName = accept;
        control.ButtonRightName = cancel;

        var result = await control.ShowAndWaitAsync(title ,message);

        control.Hide();
        control.ButtonLeftName = previousLeftButtonName;
        control.ButtonRightName = previousRightButtonName;

        return result == IDialogControl.ButtonPressed.Left;*/

        //TODO

        return false;
    }
}