﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
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

    public async ValueTask<T> Mobileconf<T>(string query, string cookieString) where T : class
    {
        var url = ApiEndpoints.Mobileconf + query;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
            throw new WgTokenInvalidException();

        if (typeof(T) != typeof(string))
            return (await responseMessage.Content.ReadFromJsonAsync<T>())!;

        var response = await responseMessage.Content.ReadAsStringAsync();
        return Unsafe.As<T>(response);

    }

    public async ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.MultipleConfirmations);
        message.Content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message);
        return (await responseMessage.Content.ReadFromJsonAsync<SendConfirmationResponse>())!;
    }
}
