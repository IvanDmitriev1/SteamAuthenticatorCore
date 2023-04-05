using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddSingleton<IConfirmationService, ConfirmationService>();
        services.AddSingleton<ILoginService, LoginService>();

        services.AddSingleton(BackgroundTimerFactory.Default);
    }
}
