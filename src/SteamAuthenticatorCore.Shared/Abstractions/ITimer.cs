using System.Threading.Tasks;
using System.Threading;
using System;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ITimer
{
    ValueTask StartOrRestart(TimeSpan timeSpan, Action<CancellationToken> func);
}

public interface IValueTaskTimer : IAsyncDisposable
{
    ValueTask StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func);
}

public interface ITaskTimer : IAsyncDisposable
{
    ValueTask StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, Task> func);
}
