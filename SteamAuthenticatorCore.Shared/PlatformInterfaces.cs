using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared;

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
}