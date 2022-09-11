using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IValueTaskTimer : IAsyncDisposable
{
    ValueTask StartOrRestart(TimeSpan timeSpan, Func<CancellationToken, ValueTask> func);
}