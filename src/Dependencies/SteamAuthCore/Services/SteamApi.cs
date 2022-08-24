using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models.Internal;
using static SteamAuthCore.Models.Internal.RefreshSessionDataResponse;

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
