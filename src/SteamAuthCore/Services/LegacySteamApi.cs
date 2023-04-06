namespace SteamAuthCore.Services;

internal sealed class LegacySteamApi : ILegacySteamApi
{
    public LegacySteamApi(HttpClient client)
    {
        _client = client;

        _client.BaseAddress = new Uri(ApiEndpoints.SteamApiBase);
        _client.AddDefaultHeaders();
    }

    private readonly HttpClient _client;

    public async ValueTask<string> GetServerTime()
    {
        using var responseMessage = await _client.PostAsJsonAsync(ApiEndpoints.TwoFactorTimeQuery, "steamid=0");
        responseMessage.EnsureSuccessStatusCode();
        var timeQuery = (await responseMessage.Content.ReadFromJsonAsync<TimeQuery>())!;
        
        return timeQuery.Response.ServerTime;
    }

    public async ValueTask<RefreshSessionDataInternalResponse?> MobileAuthGetWgToken(string token, CancellationToken cancellationToken)
    {
        var pair = new KeyValuePair<string, string>("access_token", token);

        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.MobileauthGetwgtoken);
        message.Content = new FormUrlEncodedContent(new[] {pair});

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        var response = await responseMessage.Content.ReadFromJsonAsync<RefreshSessionDataResponse>(cancellationToken: cancellationToken);
        return response!.Response;
    }

    public async Task<bool> RemoveAuthenticator(KeyValuePair<string, string>[] postData)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.MobileauthGetwgtoken);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRequestRefer);
        message.Content = new FormUrlEncodedContent(postData);

        using var responseMessage = await _client.SendAsync(message);
        if (!responseMessage.IsSuccessStatusCode)
            return false;

        return await responseMessage.Content.ReadFromJsonAsync<RemoveAuthenticatorResponse>() is not {Response.Success: true};
    }
}
