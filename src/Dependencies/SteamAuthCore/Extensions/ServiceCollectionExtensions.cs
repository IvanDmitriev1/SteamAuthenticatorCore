using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Services;

namespace SteamAuthCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSteamAuthCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ISteamApi, Services.SteamApi>();
        services.AddScoped<ITimeAligner, TimeAligner>();

        services.AddHttpClient<Services.SteamApi>();

        return services;
    }
}
