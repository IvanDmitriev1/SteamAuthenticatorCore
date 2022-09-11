using System.Text.Json.Serialization;

namespace SteamAuthCore.Models.Internal;

internal class ConfirmationDetailsResponse
{
    [JsonPropertyName("success")]
    public bool Success
    {
        get; set;
    }

    [JsonPropertyName("html")]
    public string Html { get; set; } = null!;
}
