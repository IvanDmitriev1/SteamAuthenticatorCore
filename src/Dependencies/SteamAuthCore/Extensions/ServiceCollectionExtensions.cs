using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Implementations;

namespace SteamAuthCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSteamAuthCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ISteamApi, Implementations.SteamApi>();
        services.AddScoped<ITimeAligner, TimeAligner>();

        services.AddHttpClient<Implementations.SteamApi>();

        return services;
    }
}
