using Microsoft.Extensions.DependencyInjection;

namespace SteamAuthCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSteamAuthCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ITimeAligner, TimeAligner>();
        services.AddSingleton<ILegacySteamApi, LegacySteamApi>();
        services.AddSingleton<ILegacySteamCommunityApi, LegacySteamCommunityApi>();

        services.AddSingleton<ISteamGuardAccountService, LegacySteamGuardAccountService>();

        services.AddHttpClient<ILegacySteamApi, LegacySteamApi>();
        services.AddHttpClient<ILegacySteamCommunityApi, LegacySteamCommunityApi>();

        return services;
    }
}
