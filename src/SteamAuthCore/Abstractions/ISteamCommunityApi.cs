using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Abstractions;

internal interface ISteamCommunityApi
{
    ValueTask<T> MobileConf<T>(string query, string cookieString, CancellationToken cancellationToken) where T : class;
    ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken);
    ValueTask<string> Login(string cookieString);
    ValueTask<RsaResponse?> GetRsaKey(KeyValuePair<string, string>[] content, string cookieString);
    ValueTask<LoginResponse?> DoLogin(KeyValuePair<string, string>[] content, string cookieString);
}
