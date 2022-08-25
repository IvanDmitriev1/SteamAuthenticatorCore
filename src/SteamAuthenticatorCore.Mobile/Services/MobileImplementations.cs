using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Mobile.Services.Interfaces;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileImplementations : IPlatformImplementations
{
    public MobileImplementations(IEnvironment environment)
    {
        _environment = environment;
    }

    private readonly IEnvironment _environment;

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

    public void SetTheme(Theme theme)
    {
        Application.Current.UserAppTheme = theme switch
        {
            Theme.System => OSAppTheme.Unspecified,
            Theme.Light => OSAppTheme.Light,
            Theme.Dark => OSAppTheme.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        if (Application.Current.RequestedTheme == OSAppTheme.Dark)
            _environment.SetStatusBarColor(Color.Black, false);
        else
            _environment.SetStatusBarColor(Color.White, true);
    }
}