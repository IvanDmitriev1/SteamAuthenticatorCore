namespace SteamAuthCore.Services;

internal sealed class LegacySteamApi : ILegacySteamApi
{
    public LegacySteamApi(HttpClient client)
    {
        _client = client;
    }

    private readonly HttpClient _client;

    public async ValueTask<string> GetServerTime()
    {
        using var responseMessage = await _client.PostAsJsonAsync(ApiEndpoints.TwoFactorTimeQuery, "steamid=0");
        responseMessage.EnsureSuccessStatusCode();
        var timeQuery = (await responseMessage.Content.ReadFromJsonAsync<TimeQuery>())!;
        
        return timeQuery.Response.ServerTime;
    }

    public Task<bool> RemoveAuthenticator(KeyValuePair<string, string>[] postData)
    {
        return Task.FromResult(false);
    }
}
