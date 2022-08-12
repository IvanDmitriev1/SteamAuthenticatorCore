using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IPlatformTimer : IDisposable
{
    public void Start(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func);
    public void Stop();
}