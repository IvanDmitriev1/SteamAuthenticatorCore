namespace SteamAuthCore.Models.Internal;

internal class BeginAuthSessionViaCredentials
{
	internal class AllowedConfirmation
	{
		[JsonPropertyName("confirmation_type")]
		public int ConfirmationType { get; set; }
	}

	internal class ResponseData
	{
		[JsonPropertyName("client_id")]
		public required string ClientId { get; set; }

		[JsonPropertyName("request_id")]
		public required string RequestId { get; set; }

		//public object interval { get; set; }

		[JsonPropertyName("allowed_confirmations")]
		public List<AllowedConfirmation>? AllowedConfirmations { get; set; }

		[JsonPropertyName("steamid")]
		public required string Steamid { get; set; }

		[JsonPropertyName("weak_token")]
		public required string WeakToken { get; set; }

		[JsonPropertyName("extended_error_message")]
		public required string ExtendedErrorMessage { get; set; }
	}

	[JsonPropertyName("response")]
	public required ResponseData Response { get; set; }
}