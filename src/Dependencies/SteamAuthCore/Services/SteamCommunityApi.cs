using System;
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

    public async ValueTask<string> Mobileconf(string query, string cookieString)
    {
        var url = ApiEndpoints.Mobileconf + query;

        var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
            throw new WgTokenInvalidException();

        var response = await responseMessage.Content.ReadAsStringAsync();
        return response;
    }
}
