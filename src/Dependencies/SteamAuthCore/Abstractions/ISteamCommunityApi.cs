using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SteamAuthCore.Abstractions;

internal interface ISteamCommunityApi
{
    ValueTask<string> Mobileconf(string query, CookieContainer cookies);
}
