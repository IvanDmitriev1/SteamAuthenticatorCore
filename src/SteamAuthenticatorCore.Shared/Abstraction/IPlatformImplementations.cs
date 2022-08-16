using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IPlatformImplementations
{
    public object CreateImage(string imageSource);
    public void InvokeMainThread(Action method);
    public Task DisplayAlert(string message);
}