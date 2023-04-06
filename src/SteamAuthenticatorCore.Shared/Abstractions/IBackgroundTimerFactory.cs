namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IBackgroundTimerFactory
{
    IBackgroundTimer InitializeTimer(Action<CancellationToken> func);
    IBackgroundTimer InitializeTimer(Func<CancellationToken, Task> func);

    IBackgroundTimer StartNewTimer(TimeSpan timeSpan, Action<CancellationToken> func);
}