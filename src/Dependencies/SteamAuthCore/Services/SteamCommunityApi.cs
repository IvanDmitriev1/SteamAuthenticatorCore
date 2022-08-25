using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Exceptions;
using SteamAuthCore.Extensions;
using SteamAuthCore.Models.Internal;

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

    public async ValueTask<T> Mobileconf<T>(string query, string cookieString, CancellationToken cancellationToken) where T : class
    {
        var url = ApiEndpoints.Mobileconf + query;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!responseMessage.IsSuccessStatusCode)
            throw new WgTokenInvalidException();

        if (typeof(T) != typeof(string))
            return (await responseMessage.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false))!;

        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        return Unsafe.As<T>(response);
    }

    public async ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.MultipleConfirmations);
        message.Content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        return (await responseMessage.Content.ReadFromJsonAsync<SendConfirmationResponse>(cancellationToken: cancellationToken).ConfigureAwait(false))!;
    }

    public async ValueTask<string> Login(string cookieString)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, ApiEndpoints.Login);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRequestRefer);
        message.Headers.Add("Cookie", cookieString);
        message.Headers.Add("X-Requested-With", "com.valvesoftware.android.steam.community");

        using var responseMessage = await _client.SendAsync(message).ConfigureAwait(false);
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

        using var responseMessage = await _client.SendAsync(message).ConfigureAwait(false);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        return await responseMessage.Content.ReadFromJsonAsync<RsaResponse>().ConfigureAwait(false);
    }

    public async ValueTask<LoginResponse?> DoLogin(KeyValuePair<string, string>[] content, string cookieString)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.GetRsaKey);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRequestRefer);
        message.Headers.Add("Cookie", cookieString);
        message.Content = new FormUrlEncodedContent(content);

        using var responseMessage = await _client.SendAsync(message).ConfigureAwait(false);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        return await responseMessage.Content.ReadFromJsonAsync<LoginResponse>().ConfigureAwait(false);
    }
}
