using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamApi
{
    ValueTask<string> GetServerTime();
    ValueTask<RefreshSessionDataInternalResponse?> MobileAuthGetWgToken(string token, CancellationToken cancellationToken);
    Task<bool> RemoveAuthenticator(KeyValuePair<string, string>[] postData);
}
