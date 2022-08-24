using System.Threading.Tasks;

namespace SteamAuthCore.Abstractions;

internal interface ISteamCommunityApi
{
    ValueTask<string> Mobileconf(string query, string cookieString);
}
