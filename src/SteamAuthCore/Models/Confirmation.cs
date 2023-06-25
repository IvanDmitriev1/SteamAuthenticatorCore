namespace SteamAuthCore.Models;

public class Confirmation
{
	[JsonPropertyName("type")]
	public ConfirmationType Type { get; set; }

	[JsonPropertyName("id")]
	public required string Id { get; set; }

	[JsonPropertyName("creator_id")]
	public required string CreatorId { get; set; }

	[JsonPropertyName("nonce")]
	public required string Nonce { get; set; }

	[JsonPropertyName("creation_time")]
	public long CreationTime { get; set; }

	[JsonPropertyName("cancel")]
	public required string Cancel { get; set; }

	[JsonPropertyName("accept")]
	public required string Accept { get; set; }

	[JsonPropertyName("icon")]
	public required string Icon { get; set; }

	[JsonPropertyName("multi")]
	public bool Multi { get; set; }

	[JsonPropertyName("headline")]
	public required string Headline { get; set; }

	[JsonPropertyName("summary")]
	public required List<string> Summary { get; set; }

	[JsonPropertyName("warn")]
	public required object Warn { get; set; }

	[JsonIgnore]
	public object? BitMapIcon { get; set; }

	[JsonIgnore]
	public DateTime CreateDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTime).LocalDateTime;
}