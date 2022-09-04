using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.MobileMaui.Services;

public class PlatformImplementations : IPlatformImplementations
{
    public object CreateImage(string imageSource) => throw new NotImplementedException();
    public Task DisplayAlert(string message) => throw new NotImplementedException();
    public ValueTask InvokeMainThread(Action method) => throw new NotImplementedException();
    public void SetTheme(Theme theme) => throw new NotImplementedException();
}
