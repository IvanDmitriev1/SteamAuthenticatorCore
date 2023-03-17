using Wpf.Ui.Contracts;

namespace SteamAuthenticatorCore.Desktop.Helpers;

public static class SnackbarService
{
    public static ISnackbarService Default { get; } = new Wpf.Ui.Services.SnackbarService();
}

public static class ThemeService
{
    public static IThemeService Default { get; } = new Wpf.Ui.Services.ThemeService();
}

public static class ContentDialogService
{
    public static IContentDialogService Default { get; } = new Wpf.Ui.Services.ContentDialogService();
}