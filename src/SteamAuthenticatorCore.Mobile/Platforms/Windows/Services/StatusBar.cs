using SteamAuthenticatorCore.Mobile.Abstractions;

namespace SteamAuthenticatorCore.Mobile.Services;

public class StatusBar : IStatusBar
{
    public void SetStatusBarColor(Color color, bool darkStatusBarTint) => throw new NotImplementedException();
    public void SetStatusBarColorBasedOnAppTheme() => throw new NotImplementedException();
}
