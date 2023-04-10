namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamApi
{
    ValueTask<string> GetServerTime();
    ValueTask<RefreshSessionDataInternalResponse?> MobileAuthGetWgToken(string token, CancellationToken cancellationToken);
    Task<bool> RemoveAuthenticator(KeyValuePair<string, string>[] postData);
}
