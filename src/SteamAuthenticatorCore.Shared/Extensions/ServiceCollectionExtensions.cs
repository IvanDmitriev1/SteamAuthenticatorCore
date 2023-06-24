using Microsoft.Extensions.DependencyInjection;

namespace SteamAuthenticatorCore.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddSingleton<IConfirmationService, ConfirmationService>();

        services.AddSingleton(BackgroundTimerFactory.Default);
    }
}
