using System.Drawing;

namespace SteamAuthenticatorCore.Mobile.Helpers;

public interface IEnvironment
{
    public void SetStatusBarColor(Color color, bool darkStatusBarTint);
}