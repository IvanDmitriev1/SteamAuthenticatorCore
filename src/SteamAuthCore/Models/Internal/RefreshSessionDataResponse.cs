﻿namespace SteamAuthCore.Models.Internal;

internal sealed class RefreshSessionDataResponse
{
    [JsonPropertyName("response")]
    public RefreshSessionDataInternalResponse? Response { get; set; }
}