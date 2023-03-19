﻿using System.Net.Http;
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

        services.AddHttpClient<ISteamApi, Services.SteamApi>();
        services.AddHttpClient<ISteamCommunityApi, SteamCommunityApi>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            AllowAutoRedirect = false
        });

        return services;
    }
}