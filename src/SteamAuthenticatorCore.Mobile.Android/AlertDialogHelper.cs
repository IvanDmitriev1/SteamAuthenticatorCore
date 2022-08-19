#nullable enable
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace SteamMobileAuthenticatorCore.Droid;

internal class AlertDialogHelper : IDisposable
{
    public enum ButtonPressed
    {
        None,
        Button1,
        Button2,
    }

    public AlertDialogHelper(AlertDialog.Builder builder, string title)
    {
        _alertDialog = builder.Create()!;
        _alertDialog.SetTitle(title);
        _alertDialog.DismissEvent += AlertDialogOnDismissEvent;
    }

    private readonly AlertDialog _alertDialog;
    private TaskCompletionSource<ButtonPressed>? _taskCompletionSource;

    public Task<ButtonPressed> ShowAndWait(string message, string button1, string? button2 = null)
    {
        _alertDialog.SetMessage(message);
        _alertDialog.SetButton(button1, Button1Handler);
        

        if (button2 is not null)
            _alertDialog.SetButton2(button2, Button2Handler);

        _alertDialog.Show();

        _taskCompletionSource ??= new TaskCompletionSource<ButtonPressed>();
        return _taskCompletionSource.Task;
    }

    public void Dispose()
    {
        _alertDialog.DismissEvent -= AlertDialogOnDismissEvent;
        _alertDialog.Dispose();
    }

    private void AlertDialogOnDismissEvent(object sender, EventArgs e)
    {
        _taskCompletionSource?.TrySetResult(ButtonPressed.None);
    }

    private void Button1Handler(object sender, DialogClickEventArgs e)
    {
        _alertDialog.Hide();

        _taskCompletionSource?.TrySetResult(ButtonPressed.Button1);
    }

    private void Button2Handler(object sender, DialogClickEventArgs e)
    {
        _alertDialog.Hide();

        _taskCompletionSource?.TrySetResult(ButtonPressed.Button2);
    }
}