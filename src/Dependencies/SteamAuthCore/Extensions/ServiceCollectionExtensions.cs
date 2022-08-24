using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Services;

namespace SteamAuthCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSteamAuthCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ISteamApi, Services.SteamApi>();
        services.AddScoped<ISteamCommunityApi, SteamCommunityApi>();
        services.AddScoped<ITimeAligner, TimeAligner>();
        services.AddScoped<ISteamGuardAccountService, SteamGuardAccountService>();

        services.AddHttpClient<Services.SteamApi>();
        services.AddHttpClient<SteamCommunityApi>();

        return services;
    }
}
