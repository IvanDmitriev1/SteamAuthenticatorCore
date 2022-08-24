using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Extensions;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Services;

internal sealed class SteamApi : ISteamApi
{
    public SteamApi(HttpClient client)
    {
        _client = client;

        _client.BaseAddress = new Uri(ApiEndpoints.SteamApiBase);
        _client.AddDefaultHeaders();
    }

    private readonly HttpClient _client;

    public async ValueTask<string> GetSteamTime()
    {
        using var responseMessage = await _client.PostAsJsonAsync(ApiEndpoints.TwoFactorTimeQuery, "steamid=0");
        responseMessage.EnsureSuccessStatusCode();
        var timeQuery = (await responseMessage.Content.ReadFromJsonAsync<TimeQuery>())!;
        
        return timeQuery.Response.ServerTime;
    }

    public async ValueTask<RefreshSessionDataInternalResponse?> MobileauthGetwgtoken(string token)
    {
        var query = $"{WebUtility.UrlEncode("access_token")}={WebUtility.UrlEncode(token)}";

        using var responseMessage = await _client.PostAsync(ApiEndpoints.MobileauthGetwgtoken, new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded"));
        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadFromJsonAsync<RefreshSessionDataResponse>();
        return response!.Response;
    }
}
