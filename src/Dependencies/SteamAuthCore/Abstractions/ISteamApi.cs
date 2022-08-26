using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Abstractions;

internal interface ISteamApi
{
    /// <summary>
    /// Returns server time
    /// </summary>
    /// <returns></returns>
    ValueTask<string> GetSteamTime();
    ValueTask<RefreshSessionDataInternalResponse?> MobileauthGetwgtoken(string token, CancellationToken cancellationToken);
    Task<bool> RemoveAuthenticator(KeyValuePair<string, string>[] postData);
}
