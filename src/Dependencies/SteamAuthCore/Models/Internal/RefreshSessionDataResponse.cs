using System.Text.Json.Serialization;

namespace SteamAuthCore.Models.Internal;

internal sealed class RefreshSessionDataResponse
{
    [JsonPropertyName("response")]
    public RefreshSessionDataInternalResponse? Response { get; set; }


    internal sealed class RefreshSessionDataInternalResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;

        [JsonPropertyName("token_secure")]
        public string TokenSecure { get; set; } = null!;
    }
}