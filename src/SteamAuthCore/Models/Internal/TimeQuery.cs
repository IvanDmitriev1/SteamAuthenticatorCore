namespace SteamAuthCore.Models.Internal;

internal class TimeQuery
{
    [JsonPropertyName("response")]
    public TimeQueryResponse Response { get; set; } = null!;

    internal class TimeQueryResponse
    {
        [JsonPropertyName("server_time")]
        public string ServerTime { get; set; } = null!;
    }
}