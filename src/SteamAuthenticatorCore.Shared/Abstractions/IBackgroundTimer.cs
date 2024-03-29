﻿namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IBackgroundTimer : IAsyncDisposable
{
    bool IsRunning { get; }

    ValueTask StartOrRestart(TimeSpan timeSpan);
    ValueTask Stop();
}