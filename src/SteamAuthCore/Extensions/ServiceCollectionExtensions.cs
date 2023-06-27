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

        services.AddHttpClient<ILegacySteamApi, LegacySteamApi>(client =>
        {
            AddDefaultHeaders(client);
            client.BaseAddress = new Uri(ApiEndpoints.SteamApiBase);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        services.AddHttpClient<ILegacySteamCommunityApi, LegacySteamCommunityApi>(client =>
        {
            AddDefaultHeaders(client);
            client.BaseAddress = new Uri(ApiEndpoints.CommunityBase);
            client.DefaultRequestHeaders.Referrer = new Uri(ApiEndpoints.CommunityBase);

        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        return services;
    }

    private static void AddDefaultHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/javascript;q=0.9, */*;q=0.5");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");

        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
    }
}
