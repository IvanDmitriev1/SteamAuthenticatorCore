namespace SteamAuthenticatorCore.Mobile.Abstractions;

public interface IEnvironment
{
    void SetStatusBarColor(Color color, bool darkStatusBarTint);
    void SetStatusBarColorBasedOnAppTheme();
}
