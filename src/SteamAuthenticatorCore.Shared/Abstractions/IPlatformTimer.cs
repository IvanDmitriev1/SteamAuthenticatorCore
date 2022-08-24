using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IPlatformTimer : IDisposable
{
    void Initialize(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func);
    void Start();
    void Stop();
}