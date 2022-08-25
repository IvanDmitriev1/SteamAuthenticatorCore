using System.Collections.Generic;
using System.Threading.Tasks;
using SteamAuthCore.Models.Internal;

namespace SteamAuthCore.Abstractions;

internal interface ISteamCommunityApi
{
    ValueTask<T> Mobileconf<T>(string query, string cookieString) where T : class;
    ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString);
    ValueTask<string> Login(string cookieString);
    ValueTask<RsaResponse?> GetRsaKey(KeyValuePair<string, string>[] content, string cookieString);
    ValueTask<LoginResponse?> DoLogin(KeyValuePair<string, string>[] content, string cookieString);
}
