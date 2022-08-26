using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IPlatformImplementations
{
    object CreateImage(string imageSource);
    ValueTask InvokeMainThread(Action method);
    Task DisplayAlert(string message);
    void SetTheme(Theme theme);
}