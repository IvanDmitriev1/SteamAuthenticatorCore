namespace SteamAuthenticatorCore.Shared.Services;

public sealed class BackgroundTimerFactory : IBackgroundTimerFactory
{
    public static IBackgroundTimerFactory Default { get; } = new BackgroundTimerFactory();

    public IBackgroundTimer InitializeTimer(Action<CancellationToken> func)
    {
        return new BackgroundTimer(func);
    }

    public IBackgroundTimer InitializeTimer(Func<CancellationToken, Task> func)
    {
        return new BackgroundTaskTimer(func);
    }

    public IBackgroundTimer StartNewTimer(TimeSpan timeSpan, Action<CancellationToken> func)
    {
        var timer = new BackgroundTimer(func);
        timer.Start(timeSpan);

        return timer;
    }
}