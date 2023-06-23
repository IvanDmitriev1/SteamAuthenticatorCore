namespace SteamAuthCore.Models.Internal;

internal class PollAuthSessionStatus
{
	[JsonPropertyName("response")]
	public required ResponseData Response { get; set; }

	public class ResponseData
	{
		[JsonPropertyName("refresh_token")]
		public string? RefreshToken { get; set; }

		[JsonPropertyName("access_token")]
		public string? AccessToken { get; set; }

		[JsonPropertyName("had_remote_interaction")]
		public required bool HadRemoteInteraction { get; set; }

		[JsonPropertyName("account_name")]
		public required string AccountName { get; set; }
	}

}