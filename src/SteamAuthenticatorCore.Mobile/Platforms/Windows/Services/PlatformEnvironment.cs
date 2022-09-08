using SteamAuthenticatorCore.Mobile.Abstractions;

namespace SteamAuthenticatorCore.Mobile.Services;

public class PlatformEnvironment : IEnvironment
{
    public void SetStatusBarColor(Color color, bool darkStatusBarTint) => throw new NotImplementedException();
    public void SetStatusBarColorBasedOnAppTheme() => throw new NotImplementedException();
}
