using System.Threading.Tasks;

namespace SteamAuthCore.Abstractions;

internal interface ISteamApi
{
    /// <summary>
    /// Returns server time
    /// </summary>
    /// <returns></returns>
    ValueTask<string> GetSteamTime();
}
