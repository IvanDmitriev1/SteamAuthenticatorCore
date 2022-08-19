using System.Drawing;

namespace SteamAuthenticatorCore.Mobile.Services.Interfaces;

public interface IEnvironment
{
    public void SetStatusBarColor(Color color, bool darkStatusBarTint);
}