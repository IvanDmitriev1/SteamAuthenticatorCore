using System;
using System.Threading.Tasks;
using System.Threading;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ITaskTimer : IAsyncDisposable
{
    ValueTask StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, Task> func);
}
