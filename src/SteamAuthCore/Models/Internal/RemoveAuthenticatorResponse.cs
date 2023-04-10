namespace SteamAuthCore.Models.Internal;

internal class RemoveAuthenticatorResponse
{
    [JsonPropertyName("response")]
    public RemoveAuthenticatorInternalResponse? Response { get; set; }

    internal class RemoveAuthenticatorInternalResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
