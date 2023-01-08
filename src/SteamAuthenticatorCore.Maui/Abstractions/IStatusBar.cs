namespace SteamAuthenticatorCore.Mobile.Abstractions;

public interface IStatusBar
{
    void SetStatusBarColor(Color color, bool darkStatusBarTint);
    void SetStatusBarColorBasedOnAppTheme();
}
