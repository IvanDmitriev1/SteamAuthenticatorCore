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

    public async ValueTask<GetListJson> MobileConf(string query, string cookieString, CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.Mobileconf + query;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        if (await responseMessage.Content.ReadFromJsonAsync<GetListJson>(cancellationToken:  cancellationToken) is not { } response)
            throw new WgTokenInvalidException();

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
}
