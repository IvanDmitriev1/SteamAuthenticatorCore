using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SteamAuthenticatorCore.Shared;
using WPFUI.Appearance;
using WPFUI.DIControls.Interfaces;
using Theme = SteamAuthenticatorCore.Shared.Theme;

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

    public void SetTheme(Theme theme)
    {
        ThemeType themeType = theme switch
        {
            Theme.System => GetThemeFromSystem(),
            Theme.Light => ThemeType.Light,
            Theme.Dark => ThemeType.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        WPFUI.Appearance.Theme.Set(themeType);
    }

    private static ThemeType GetThemeFromSystem()
    {
        var  systemThem = WPFUI.Appearance.Theme.GetSystemTheme();

        var themeToSet = ThemeType.Light;

        if (systemThem is SystemThemeType.Dark or SystemThemeType.CapturedMotion or SystemThemeType.Glow)
            themeToSet = ThemeType.Dark;

        return themeToSet;
    }
}