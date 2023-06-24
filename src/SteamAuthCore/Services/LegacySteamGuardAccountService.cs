
namespace SteamAuthCore.Services;

internal class LegacySteamGuardAccountService : ISteamGuardAccountService
{
    public LegacySteamGuardAccountService(ILegacySteamApi legacySteamApi, ILegacySteamCommunityApi legacySteamCommunityApi)
    {
        _legacySteamApi = legacySteamApi;
        _legacySteamCommunityApi = legacySteamCommunityApi;
    }

    private readonly ILegacySteamApi _legacySteamApi;
    private readonly ILegacySteamCommunityApi _legacySteamCommunityApi;

    public async Task<IReadOnlyList<Confirmation>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        var builder = new StringBuilder(140 + 5);
        builder.Append(GenerateConfirmationQueryParams(account, "conf"));

        var confirmationsListJson = await _legacySteamCommunityApi
                                          .MobileConf(builder.ToString(), account.Session.GetCookieString(), cancellationToken)
                                          .ConfigureAwait(false);

        return confirmationsListJson.Conf ?? new List<Confirmation>();
    }

    public Task<bool> SendConfirmation(SteamGuardAccount account, Confirmation confirmation, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        var strOption = options.ToString().ToLower();

        var builder = new StringBuilder(140 + 50);
        builder.Append($"?op={strOption}");
        builder.Append('&');
        builder.Append(GenerateConfirmationQueryParams(account, strOption));
        builder.Append($"&cid={confirmation.Id}");
        builder.Append($"&ck={confirmation.Nonce}");

        return _legacySteamCommunityApi.SendSingleConfirmations(builder.ToString(), account.Session.GetCookieString(), cancellationToken);
    }

    public Task<bool> SendConfirmation(SteamGuardAccount account, IReadOnlyList<Confirmation> confirmations, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        var strOption = options.ToString().ToLower();
        var capacity = 140 + confirmations.Count * (20 + 11 + 7 + 6 + 3);

        var builder = new StringBuilder(capacity);
        builder.Append($"op={strOption}");
        builder.Append('&');
        builder.Append(GenerateConfirmationQueryParams(account, strOption));

        foreach (var confirmation in confirmations)
        {
            builder.Append($"&cid[]={confirmation.Id}");
            builder.Append($"&ck[]={confirmation.Nonce}");
        }

        return _legacySteamCommunityApi.SendMultipleConfirmations(builder.ToString(), account.Session.GetCookieString(), cancellationToken);
    }

    internal static string GenerateConfirmationQueryParams(SteamGuardAccount account, string tag)
    {
        var time = ITimeAligner.SteamTime;

        var builder = new StringBuilder(140);
        builder.Append($"p={account.DeviceId}");
        builder.Append($"&a={account.Session.SteamId}");
        builder.Append($"&k={GenerateConfirmationHashForTime(time, tag, account.IdentitySecret)}");
        builder.Append($"&t={time}");
        builder.Append("&m=android");
        builder.Append($"&tag={tag}");

        return builder.ToString();
    }

    private static string? GenerateConfirmationHashForTime(Int64 time, string tag, string identitySecret)
    {
        var n2 = 8 + tag.Length;

        Span<byte> array = stackalloc byte[n2];
        var n3 = 8;

        while (true)
        {
            int n4 = n3 - 1;

            if (n3 <= 0)
                break;

            array[n4] = (byte)time;
            time >>= 8;
            n3 = n4;
        }


        Span<byte> buffer = stackalloc byte[tag.Length];
        Encoding.UTF8.GetBytes(tag, buffer);

        int j = 8;
        for (int i = 0; i < n2 - 8; i++)
        {
            array[j] = buffer[i];

            j++;
        }

        try
        {
            Span<byte> key = stackalloc byte[20];
            Convert.TryFromBase64String(identitySecret, key, out _);

            Span<byte> hashedSpan = stackalloc byte[20];
            HMACSHA1.HashData(key, array, hashedSpan);

            var encodedData = Convert.ToBase64String(hashedSpan);
            return encodedData;
        }
        catch
        {
            return null;
        }
    }
}
