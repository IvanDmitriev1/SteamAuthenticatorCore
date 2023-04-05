using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Exceptions;
using SteamAuthCore.Extensions;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Services;

internal sealed class LegacySteamCommunityApi : ILegacySteamCommunityApi
{
    public LegacySteamCommunityApi(HttpClient client)
    {
        _client = client;

        _client.BaseAddress = new Uri(ApiEndpoints.CommunityBase);
        _client.AddDefaultHeaders();
        _client.DefaultRequestHeaders.Referrer = new Uri(ApiEndpoints.CommunityBase);
    }

    private readonly HttpClient _client;

    public async ValueTask<string> MobileConf(string query, string cookieString, CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.Mobileconf + query;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            throw new WgTokenInvalidException();

        var response = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        return response;
    }

    public async ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.MultipleConfirmations);
        message.Content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        return (await responseMessage.Content.ReadFromJsonAsync<SendConfirmationResponse>(cancellationToken: cancellationToken))!;
    }

    public async ValueTask<string> Login(string cookieString)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, ApiEndpoints.Login);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRequestRefer);
        message.Headers.Add("Cookie", cookieString);
        message.Headers.Add("X-Requested-With", "com.valvesoftware.android.steam.community");

        using var responseMessage = await _client.SendAsync(message);
        responseMessage.EnsureSuccessStatusCode();

        try
        {
            var sessionCookie = responseMessage.Headers.GetValues("Set-Cookie").ElementAt(0);
            var arr = sessionCookie.Split(new[] {'=', ';'}, StringSplitOptions.RemoveEmptyEntries);

            return arr[1];
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async ValueTask<RsaResponse?> GetRsaKey(KeyValuePair<string, string>[] content, string cookieString)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.GetRsaKey);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRequestRefer);
        message.Headers.Add("Cookie", cookieString);
        message.Content = new FormUrlEncodedContent(content);

        using var responseMessage = await _client.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        return await responseMessage.Content.ReadFromJsonAsync<RsaResponse>();
    }

    public async ValueTask<LoginResponse?> DoLogin(KeyValuePair<string, string>[] content, string cookieString)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.GetRsaKey);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRequestRefer);
        message.Headers.Add("Cookie", cookieString);
        message.Content = new FormUrlEncodedContent(content);

        using var responseMessage = await _client.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        return await responseMessage.Content.ReadFromJsonAsync<LoginResponse>();
    }
}
