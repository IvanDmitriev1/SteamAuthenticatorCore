using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IPlatformImplementations
{
    public object CreateImage(string imageSource);
    public ValueTask InvokeMainThread(Action method);
    public Task DisplayAlert(string message);
}