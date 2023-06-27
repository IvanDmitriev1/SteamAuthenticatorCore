namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamApi
{
    ValueTask<string> GetServerTime();
    Task<bool> RemoveAuthenticator(KeyValuePair<string, string>[] postData);
}
