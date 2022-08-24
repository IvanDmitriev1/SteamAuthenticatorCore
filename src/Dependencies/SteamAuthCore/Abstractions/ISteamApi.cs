using System.Threading.Tasks;
using static SteamAuthCore.Models.Internal.RefreshSessionDataResponse;

namespace SteamAuthCore.Abstractions;

internal interface ISteamApi
{
    /// <summary>
    /// Returns server time
    /// </summary>
    /// <returns></returns>
    ValueTask<string> GetSteamTime();

    ValueTask<RefreshSessionDataInternalResponse?> MobileauthGetwgtoken(string token);
}
