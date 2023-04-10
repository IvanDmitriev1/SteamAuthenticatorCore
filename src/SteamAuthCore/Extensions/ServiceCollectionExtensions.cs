using Microsoft.Extensions.DependencyInjection;

namespace SteamAuthCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSteamAuthCoreServices(this IServiceCollection services, bool isMobile)
    {
        services.AddSingleton<ITimeAligner, TimeAligner>();
        services.AddSingleton<ILegacySteamApi, LegacySteamApi>();
        services.AddSingleton<ILegacySteamCommunityApi, LegacySteamCommunityApi>();

        services.AddSingleton<LegacySteamGuardAccountService>();
        services.AddSingleton<UltraLegacySteamGuardAccountService>();

        if (isMobile)
            services.AddSingleton<ISteamGuardAccountService>(provider =>
                provider.GetRequiredService<UltraLegacySteamGuardAccountService>());
        else
            services.AddSingleton<ISteamGuardAccountService>(provider =>
                provider.GetRequiredService<LegacySteamGuardAccountService>());

        services.AddHttpClient<ILegacySteamApi, LegacySteamApi>();
        services.AddHttpClient<ILegacySteamCommunityApi, LegacySteamCommunityApi>();

        return services;
    }
}
