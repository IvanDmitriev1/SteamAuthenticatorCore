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
    public SteamGuardAccountService(ISteamApi steamApi, ISteamCommunityApi steamCommunityApi)
    {
        _steamApi = steamApi;
        _steamCommunityApi = steamCommunityApi;
    }

    private readonly ISteamApi _steamApi;
    private readonly ISteamCommunityApi _steamCommunityApi;
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
            if (!await RefreshSession(account))
            {
                return Enumerable.Empty<ConfirmationModel>();
            }

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

    public async Task<LoginResult> Login(LoginData loginData)
    {
        if (string.IsNullOrEmpty(loginData.SessionId))
        {
            loginData.SessionId = await _steamCommunityApi.Login(LoginData.DefaultCookies);

            var cookieBuilder = new StringBuilder();
            cookieBuilder.Append($"sessionid={loginData.SessionId};");
            cookieBuilder.Append(LoginData.DefaultCookies);

            loginData.CookieString = cookieBuilder.ToString();
        }

        var getRsaKeyContent = new KeyValuePair<string, string>[2];
        getRsaKeyContent[0] = new KeyValuePair<string, string>("donotcache", $"{ITimeAligner.SteamTime * 1000}");
        getRsaKeyContent[1] = new KeyValuePair<string, string>("username", loginData.Username);

        
        if (await _steamCommunityApi.GetRsaKey(getRsaKeyContent, loginData.CookieString) is not { } rsaResponse)
        {
            loginData.Result = LoginResult.GeneralFailure;
            return loginData.Result;
        }

        if (!rsaResponse.Success)
        {
            loginData.Result = LoginResult.BadRsa;
            return loginData.Result;
        }

        string encryptedPassword;
        using (var rsaEncryptor = new RSACryptoServiceProvider())
        {
            var passwordBytes = Encoding.ASCII.GetBytes(loginData.Password);
            var rsaParameters = rsaEncryptor.ExportParameters(false);
            rsaParameters.Exponent = Util.HexStringToByteArray(rsaResponse.Exponent);
            rsaParameters.Modulus = Util.HexStringToByteArray(rsaResponse.Modulus);
            rsaEncryptor.ImportParameters(rsaParameters);

            encryptedPassword = Convert.ToBase64String(rsaEncryptor.Encrypt(passwordBytes, false));
        }

        var doLoginPostData = new KeyValuePair<string, string>[13];
        doLoginPostData[0] = new KeyValuePair<string, string>("donotcache", $"{ITimeAligner.SteamTime * 1000}");

        doLoginPostData[1] = new KeyValuePair<string, string>("password", encryptedPassword);
        doLoginPostData[2] = new KeyValuePair<string, string>("username", loginData.Username);
        doLoginPostData[3] = new KeyValuePair<string, string>("twofactorcode", loginData.TwoFactorCode);

        doLoginPostData[4] = new KeyValuePair<string, string>("emailauth", string.Empty);
        doLoginPostData[5] = new KeyValuePair<string, string>("loginfriendlyname", string.Empty);
        doLoginPostData[6] = new KeyValuePair<string, string>("captchagid", loginData.CaptchaGid ?? "-1");
        doLoginPostData[7] = new KeyValuePair<string, string>("captcha_text", loginData.CaptchaGid is null ? string.Empty : loginData.CaptchaText);
        doLoginPostData[8] = new KeyValuePair<string, string>("emailsteamid", loginData.Result == LoginResult.Need2Fa ? loginData.SteamId.ToString() : string.Empty);

        doLoginPostData[9] = new KeyValuePair<string, string>("rsatimestamp", rsaResponse.Timestamp);
        doLoginPostData[10] = new KeyValuePair<string, string>("remember_login", "true");
        doLoginPostData[11] = new KeyValuePair<string, string>("oauth_client_id", "DE45CD61");
        doLoginPostData[12] = new KeyValuePair<string, string>("oauth_scope", "read_profile write_profile read_client write_client");

        if (await _steamCommunityApi.DoLogin(doLoginPostData, loginData.CookieString) is not { } loginResponse)
        {
            loginData.Result = LoginResult.GeneralFailure;
            return loginData.Result;   
        }

        if (loginResponse.LoginComplete)
        {
            var oAuthData = loginResponse.OAuthData!;

            loginData.SessionData = new SessionData(loginData.SessionId,
                oAuthData.SteamId + "%7C%7C" + oAuthData.SteamLogin,
                oAuthData.SteamId + "%7C%7C" + oAuthData.SteamLogin,
                oAuthData.Webcookie,
                oAuthData.OAuthToken!, loginData.SteamId);

            loginData.Result = LoginResult.LoginOkay;
            return loginData.Result;
        }

        if (loginResponse.Message is null)
        {
            loginData.Result = LoginResult.GeneralFailure;
            return loginData.Result;
        }

        if (loginResponse.Message.Contains("There have been too many login failures"))
        {
            loginData.Result = LoginResult.TooManyFailedLogins;
            return loginData.Result;
        }

        if (loginResponse.Message.Contains("Incorrect login"))
        {
            loginData.Result = LoginResult.BadCredentials;
            return loginData.Result;
        }

        if (loginResponse.CaptchaNeeded)
        {
            loginData.CaptchaGid = loginResponse.CaptchaGid;
            loginData.Result = LoginResult.NeedCaptcha;
            return loginData.Result;
        }

        if (loginResponse.EmailAuthNeeded)
        {
            throw new NotImplementedException();
        }

        if (loginResponse.TwoFactorNeeded && !loginResponse.Success)
        {
            loginData.Result = LoginResult.Need2Fa;
            return loginData.Result;
        }

        if (loginResponse.OAuthData?.OAuthToken is null || loginResponse.OAuthData.OAuthToken.Length == 0)
        {
            loginData.Result = LoginResult.GeneralFailure;
            return loginData.Result;
        }

        return loginData.Result;
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

    private static string GenerateConfirmationQueryParams(SteamGuardAccount account, string tag)
    {
        var time = ITimeAligner.SteamTime;

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
