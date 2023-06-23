using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

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
    internal static readonly HtmlParser Parser = new();

    public async Task<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        if (await SendFetchConfirmationsRequest(account, cancellationToken) is not { } html)
            return Enumerable.Empty<ConfirmationModel>();

        using var document = await Parser.ParseDocumentAsync(html, cancellationToken);
        return document.GetElementsByClassName("mobileconf_list_entry").Select(GetConfirmationModelFromHtml);
    }

    public async Task<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        var strOption = options.ToString().ToLower();

        var builder = new StringBuilder(140 + 50);
        builder.Append("ajaxop");
        builder.Append($"?op={strOption}");
        builder.Append('&');
        builder.Append(GenerateConfirmationQueryParams(account, strOption));
        builder.Append($"&cid={confirmation.Id}");
        builder.Append($"&ck={confirmation.Key}");

        var response = await _legacySteamCommunityApi.MobileConf(builder.ToString(), account.Session.GetCookieString(), cancellationToken);
        var confirmationDetailsResponse = JsonSerializer.Deserialize<ConfirmationDetailsResponse>(response);

        return confirmationDetailsResponse?.Success == true;
    }

    public async Task<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel[] confirmations, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        var strOption = options.ToString().ToLower();
        var capacity = 140 + confirmations.Length * (20 + 11 + 7 + 6 + 3);

        var builder = new StringBuilder(capacity);
        builder.Append($"op={strOption}");
        builder.Append('&');
        builder.Append(GenerateConfirmationQueryParams(account, strOption));

        foreach (var confirmation in confirmations)
        {
            builder.Append($"&cid[]={confirmation.Id}");
            builder.Append($"&ck[]={confirmation.Key}");
        }

        var response = await _legacySteamCommunityApi.SendMultipleConfirmations(builder.ToString(), account.Session.GetCookieString(), cancellationToken);
        return response.Success;
    }

    private async ValueTask<string?> SendFetchConfirmationsRequest(SteamGuardAccount account, CancellationToken cancellationToken, UInt16 times = 0)
    {
        try
        {
            var builder = new StringBuilder(140 + 5);
            builder.Append("conf?");
            builder.Append(GenerateConfirmationQueryParams(account, "conf"));

            var html = await _legacySteamCommunityApi
                .MobileConf(builder.ToString(), account.Session.GetCookieString(), cancellationToken)
                .ConfigureAwait(false);

            if (html.Contains("Nothing to confirm"))
                return null;

            if (!html.Contains("Invalid authenticator"))
                return html;

            return await SendFetchConfirmationsRequest(account, cancellationToken);
        }
        catch (WgTokenInvalidException)
        {
            if (times >= 1)
                return null;

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            return await SendFetchConfirmationsRequest(account, cancellationToken, ++times);
        }
        catch (WgTokenExpiredException)
        {
            return null;
        }
    }

    internal static ConfirmationModel GetConfirmationModelFromHtml(IElement confirmationElement)
    {
        Span<UInt64> attributesValue = stackalloc UInt64[5];

        for (var i = 2; i <= 5; i++)
        {
            attributesValue[i - 2] = UInt64.Parse(confirmationElement.Attributes[i]!.Value);
        }

        var confirmationElementContent = confirmationElement.FirstElementChild!;
        var imageSource = ((IHtmlImageElement)confirmationElementContent.QuerySelector("img")!).Source!;
        var children = confirmationElementContent.Children[^1].Children;

        return new ConfirmationModel(attributesValue, imageSource, children);
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
