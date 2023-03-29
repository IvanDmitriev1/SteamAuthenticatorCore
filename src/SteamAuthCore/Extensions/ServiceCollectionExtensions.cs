using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Services;

namespace SteamAuthCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSteamAuthCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ILegacySteamApi, Services.LegacySteamApi>();
        services.AddScoped<ILegacySteamCommunityApi, LegacySteamCommunityApi>();
        services.AddScoped<ITimeAligner, TimeAligner>();
        services.AddScoped<ISteamGuardAccountService, LegacySteamGuardAccountService>();

        services.AddHttpClient<ILegacySteamApi, Services.LegacySteamApi>();
        services.AddHttpClient<ILegacySteamCommunityApi, LegacySteamCommunityApi>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            AllowAutoRedirect = false
        });

        return services;
    }
}
