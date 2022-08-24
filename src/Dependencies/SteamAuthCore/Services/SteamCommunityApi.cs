using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Exceptions;
using SteamAuthCore.Extensions;

namespace SteamAuthCore.Services;

internal sealed class SteamCommunityApi : ISteamCommunityApi
{
    public SteamCommunityApi(HttpClient client)
    {
        _client = client;

        _client.BaseAddress = new Uri(ApiEndpoints.CommunityBase);
        _client.AddDefaultHeaders();
        _client.DefaultRequestHeaders.Referrer = new Uri(ApiEndpoints.CommunityBase);
    }

    private readonly HttpClient _client;

    public async ValueTask<string> Mobileconf(string query, CookieContainer cookies)
    {
        var httpClientHandler = new HttpClientHandler()
        {
            CookieContainer = cookies,
            UseCookies = true
        };

        httpClientHandler.CookieContainer = cookies;
        using var client = new HttpClient(httpClientHandler, true);
        client.BaseAddress = new Uri(ApiEndpoints.CommunityBase);
        client.AddDefaultHeaders();
        client.DefaultRequestHeaders.Referrer = new Uri(ApiEndpoints.CommunityBase);

        var url = ApiEndpoints.Mobileconf + query;

        using var responseMessage = await client.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode)
            throw new WgTokenInvalidException();

        var response = await responseMessage.Content.ReadAsStringAsync();
        return response;
    }
}
