namespace SteamAuthCore.Models.Internal;

internal class SendConfirmationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
