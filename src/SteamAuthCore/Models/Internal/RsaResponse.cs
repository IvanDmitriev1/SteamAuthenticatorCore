namespace SteamAuthCore.Models.Internal;

internal class RsaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("publickey_exp")]
    public string Exponent { get; set; } = null!;

    [JsonPropertyName("publickey_mod")]
    public string Modulus { get; set; } = null!;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = null!;

    [JsonPropertyName("steamid")]
    public ulong SteamId { get; set; }
}
