using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Exceptions;
using SteamAuthCore.Models;

namespace SteamAuthCore.Services;

internal class SteamGuardAccountService : ISteamGuardAccountService
{
    public SteamGuardAccountService(ISteamApi steamApi, ISteamCommunityApi steamCommunityApi, ITimeAligner timeAligner)
    {
        _steamApi = steamApi;
        _steamCommunityApi = steamCommunityApi;
        _timeAligner = timeAligner;
    }

    private readonly ISteamApi _steamApi;
    private readonly ISteamCommunityApi _steamCommunityApi;
    private readonly ITimeAligner _timeAligner;
    private static readonly HtmlParser Parser = new();
    private readonly ArrayPool<UInt64> _uintArrayPool = ArrayPool<UInt64>.Create();
    private readonly ArrayPool<string> _stringArrayPool = ArrayPool<string>.Create();

    public async ValueTask<bool> RefreshSession(SteamGuardAccount account)
    {
        if (await _steamApi.MobileauthGetwgtoken(account.Session.OAuthToken) is not { } refreshResponse)
            return false;

        var token = account.Session.SteamId + "%7C%7C" + refreshResponse.Token;
        var tokenSecure = account.Session.SteamId + "%7C%7C" + refreshResponse.TokenSecure;

        account.Session.SteamLogin = token;
        account.Session.SteamLoginSecure = tokenSecure;
        return true;
    }

    public async ValueTask<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        var builder = new StringBuilder("conf?");
        builder.Append(GenerateConfirmationQueryParams(account, "conf"));

        var query = builder.ToString();
        string html;

        try
        {
            html = await _steamCommunityApi.Mobileconf<string>(query, account.Session.GetCookieString());
        }
        catch (WgTokenInvalidException)
        {
            await RefreshSession(account);

            try
            {
                html = await _steamCommunityApi.Mobileconf<string>(query, account.Session.GetCookieString());
            }
            catch (WgTokenInvalidException)
            {
                return Enumerable.Empty<ConfirmationModel>();
            }
        }
        catch (WgTokenExpiredException)
        {
            return Enumerable.Empty<ConfirmationModel>();
        }

        return ParseConfirmationsHtml(html);
    }

    public async ValueTask<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options)
    {
        var strOption = options.ToString().ToLower();

        var builder = new StringBuilder("ajaxop");
        builder.Append($"?op={strOption}");
        builder.Append('&');
        builder.Append(GenerateConfirmationQueryParams(account, strOption));
        builder.Append($"&cid={confirmation.Id}");
        builder.Append($"&ck={confirmation.Key}");

        var query = builder.ToString();

        var response = await _steamCommunityApi.Mobileconf<ConfirmationDetailsResponse>(query, account.Session.GetCookieString());
        return response.Success;
    }

    public async ValueTask<bool> SendConfirmation(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmations, ConfirmationOptions options)
    {
        var strOption = options.ToString().ToLower();

        var builder = new StringBuilder();
        builder.Append($"op={strOption}");
        builder.Append('&');
        builder.Append(GenerateConfirmationQueryParams(account, strOption));

        foreach (var confirmation in confirmations)
        {
            builder.Append($"&cid[]={confirmation.Id}");
            builder.Append($"&ck[]={confirmation.Key}");
        }

        var response = await _steamCommunityApi.SendMultipleConfirmations(builder.ToString(), account.Session.GetCookieString());
        return response.Success;
    }

    private IEnumerable<ConfirmationModel> ParseConfirmationsHtml(string html)
    {
        if (html.Contains("<div>Nothing to confirm</div>"))
            return Array.Empty<ConfirmationModel>();

        using var document = Parser.ParseDocument(html);

        return document.GetElementsByClassName("mobileconf_list_entry").Select(GetConfirmationModelFromHtml);
    }

    private ConfirmationModel GetConfirmationModelFromHtml(IElement confirmationElement)
    {
        var attributesValue = _uintArrayPool.Rent(5);

        for (var i = 2; i <= 5; i++)
        {
            attributesValue[i - 2] = UInt64.Parse(confirmationElement.Attributes[i]!.Value);
        }

        var confirmationElementContent = confirmationElement.FirstElementChild!;
        var imageSource = ((IHtmlImageElement)confirmationElementContent.QuerySelector("img")!).Source!;
        var children = confirmationElementContent.Children[^1].Children;

        var descriptionArray = _stringArrayPool.Rent(3);
        for (var i = 0; i < 3; i++)
        {
            var description = children[i];
            descriptionArray[i] = description.TextContent;
        }

        var model = new ConfirmationModel(attributesValue, imageSource, descriptionArray);
        _stringArrayPool.Return(descriptionArray, true);
        _uintArrayPool.Return(attributesValue);

        return model;
    }

    private string GenerateConfirmationQueryParams(SteamGuardAccount account, string tag)
    {
        var time = _timeAligner.SteamTime;

        var builder = new StringBuilder();
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
            using HMACSHA1 hmacGenerator = new(Convert.FromBase64String(identitySecret));
            var hashedData = hmacGenerator.ComputeHash(array.ToArray());
            var encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
            return WebUtility.UrlEncode(encodedData);
        }
        catch
        {
            return null;
        }
    }
}
