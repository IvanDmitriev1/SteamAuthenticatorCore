using SteamAuthenticatorCore.MobileMaui.Abstractions;
using SteamAuthenticatorCore.MobileMaui.Services;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.MobileMaui.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection services)
    {
        services.AddSingleton<IEnvironment, PlatformEnvironment>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPlatformImplementations, PlatformImplementations>();

        return services;
    }
}
