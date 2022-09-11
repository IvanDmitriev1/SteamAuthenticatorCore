﻿using SteamAuthenticatorCore.Mobile.Abstractions;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class PlatformImplementations : IPlatformImplementations
{
    public PlatformImplementations(IStatusBar statusBar)
    {
        _statusBar = statusBar;
    }

    private readonly IStatusBar _statusBar;

    public object CreateImage(string imageSource)
    {
        var image = new UriImageSource
        {
            Uri = new Uri(imageSource, UriKind.Absolute),
            CachingEnabled = true,
            CacheValidity = TimeSpan.FromDays(1)
        };

        return image;
    }

    public async ValueTask InvokeMainThread(Action method)
    {
        if (MainThread.IsMainThread)
        {
            method.Invoke();
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(method);
    }

    public Task DisplayAlert(string title, string message)
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, "Ok");
    }

    public Task<bool> DisplayPrompt(string title, string message, string accept = "Ok", string cancel = "Cancel")
    {
        return Application.Current!.MainPage!.DisplayAlert(title, message, accept, cancel);
    }

    public void SetTheme(Theme theme)
    {
        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme = theme switch
        {
            Theme.System => AppTheme.Unspecified,
            Theme.Light => AppTheme.Light,
            Theme.Dark => AppTheme.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        _statusBar.SetStatusBarColorBasedOnAppTheme();
    }
}