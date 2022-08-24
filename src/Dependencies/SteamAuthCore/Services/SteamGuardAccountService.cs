using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Exceptions;

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

        var tag = "conf";
        var time = _timeAligner.SteamTime;

        var query = "p=" + account.DeviceId + "&a=" + account.Session.SteamId + "&k=" + GenerateConfirmationHashForTime(time, tag, account.IdentitySecret) + "&t=" + time + "&m=android&tag=" + tag;

        string html;

        try
        {
            html = await _steamCommunityApi.Mobileconf(query, account.Session.GetCookies());
        }
        catch (WgTokenInvalidException)
        {
            await RefreshSession(account);

            try
            {
                html = await _steamCommunityApi.Mobileconf(query, account.Session.GetCookies());
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

    private IEnumerable<ConfirmationModel> ParseConfirmationsHtml(string html)
    {
        if (html.Contains("<div>Nothing to confirm</div>"))
            return Array.Empty<ConfirmationModel>();


        using var document = Parser.ParseDocument(html);
        List<ConfirmationModel> confirmationModels = new();

        foreach (var confirmationElement in document.GetElementsByClassName("mobileconf_list_entry"))
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

            confirmationModels.Add(new ConfirmationModel(attributesValue, imageSource, descriptionArray));
            _stringArrayPool.Return(descriptionArray, true);
            _uintArrayPool.Return(attributesValue);
        }

        return confirmationModels;
    }

    private static string? GenerateConfirmationHashForTime(Int64 time, string tag, string identitySecret)
    {
        byte[] decode = Convert.FromBase64String(identitySecret);
        int n2 = 8;

        if (tag is not null)
        {
            if (tag.Length > 32)
            {
                n2 = 8 + 32;
            }
            else
            {
                n2 = 8 + tag.Length;
            }
        }

        byte[] array = new byte[n2];
        int n3 = 8;

        while (true)
        {
            int n4 = n3 - 1;
            if (n3 <= 0)
            {
                break;
            }
            array[n4] = (byte)time;
            time >>= 8;
            n3 = n4;
        }
        if (tag is not null)
            Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);

        try
        {
            using HMACSHA1 hmacGenerator = new()
            {
                Key = decode
            };
            byte[] hashedData = hmacGenerator.ComputeHash(array);
            string encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
            return WebUtility.UrlEncode(encodedData);
        }
        catch
        {
            return null;
        }
    }
}
