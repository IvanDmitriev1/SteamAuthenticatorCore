using System.Threading.Tasks;
using System.Threading;
using System;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ITimer
{
    Task StartOrRestart(TimeSpan timeSpan, Action<CancellationToken> func);
    Task Stop();
}

public interface IValueTaskTimer : IAsyncDisposable
{
    Task StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func);
    Task Stop();
}

public interface ITaskTimer : IAsyncDisposable
{
    Task StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, Task> func);
    Task Stop();
}
