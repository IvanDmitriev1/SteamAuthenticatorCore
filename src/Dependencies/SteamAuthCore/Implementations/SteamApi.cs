using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Implementations;

internal sealed class SteamApi : ISteamApi
{
    public SteamApi(HttpClient client)
    {
        _client = client;

        _client.BaseAddress = new Uri(ApiEndpoints.SteamApiBase);
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30");
    }

    private readonly HttpClient _client;

    public async ValueTask<string> GetSteamTime()
    {
        using var response = await _client.PostAsJsonAsync(ApiEndpoints.TwoFactorTimeQuery, "steamid=0");
        response.EnsureSuccessStatusCode();
        var timeQuery = (await response.Content.ReadFromJsonAsync<TimeQuery>())!;
        
        return timeQuery.Response.ServerTime;
    }
}
