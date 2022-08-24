using System.Text.Json.Serialization;

namespace SteamAuthCore.Models.Internal;

internal class OAuth
{
    [JsonPropertyName("steamid")]
    public string SteamId { get; set; } = string.Empty;

    [JsonPropertyName("oauth_token")]
    public string? OAuthToken { get; set; }

    [JsonPropertyName("wgtoken")]
    public string SteamLogin { get; set; } = null!;

    [JsonPropertyName("wgtoken_secure")]
    public string SteamLoginSecure { get; set; } = null!;

    [JsonPropertyName("webcookie")]
    public string Webcookie { get; set; } = null!;
}