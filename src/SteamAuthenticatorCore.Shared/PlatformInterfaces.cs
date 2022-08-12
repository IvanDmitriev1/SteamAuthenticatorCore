using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared;

public enum Theme
{
    System = 0,
    Light = 1,
    Dark = 2,
}

public interface IPlatformTimer
{
    public void ChangeInterval(TimeSpan time);

    public void SetFuncOnTick(Func<Task> callback);
    public void SetFuncOnTick(Action callback);

    public void Start();
    public void Stop();
}

public interface IPlatformImplementations
{
    public object CreateImage(string imageSource);
    public void InvokeMainThread(Action method);
    public Task DisplayAlert(string message);
    public void SetTheme(Theme theme);
}