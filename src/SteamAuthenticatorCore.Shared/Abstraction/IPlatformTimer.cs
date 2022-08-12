using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IPlatformTimer
{
    public void ChangeInterval(TimeSpan time);

    public void SetFuncOnTick(Func<Task> callback);
    public void SetFuncOnTick(Action callback);

    public void Start();
    public void Stop();
}