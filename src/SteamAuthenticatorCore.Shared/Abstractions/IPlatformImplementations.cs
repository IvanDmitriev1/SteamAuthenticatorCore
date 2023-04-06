namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IPlatformImplementations
{
    object CreateImage(string imageSource);
    ValueTask InvokeMainThread(Action method);
    Task DisplayAlert(string title, string message);
    Task<bool> DisplayPrompt(string title, string message, string accept = "Ok", string cancel = "Cancel");
}