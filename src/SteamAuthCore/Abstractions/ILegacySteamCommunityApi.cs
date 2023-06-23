namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamCommunityApi
{
    ValueTask<string> MobileConf(string query, string cookieString, CancellationToken cancellationToken);
    ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken);
}
