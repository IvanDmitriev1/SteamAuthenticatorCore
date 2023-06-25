
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

    public async Task<LoginAgainData> LoginAgain(SteamGuardAccount account, string password, LoginAgainData data, CancellationToken cancellationToken)
    {
        data.SessionCookie ??= await _legacySteamCommunityApi.GenerateSessionIdCookieForLogin(cancellationToken);

        if (await _legacySteamCommunityApi.LoginGetRsaKey(account.AccountName, cancellationToken) is not { Success: true } rsaResponse)
            return new LoginAgainData(LoginResult.BadRsa);

        string encryptedPassword = EncryptPassword(password, rsaResponse);
        KeyValuePair<string, string>[] postData =
        {
            new("donotcache", $"{ITimeAligner.SteamTime * 1000}"),

            new("password", encryptedPassword),
            new("username", account.AccountName),

            new("twofactorcode", data.LoginResult == LoginResult.Need2Fa ? account.GenerateSteamGuardCode() : string.Empty),
            new("emailsteamid", string.Empty),

            new("emailauth", data.EmailCode ?? string.Empty),
            new("captchagid", data.CaptchaGid ?? "-1"),
            new("captcha_text", data.CaptchaText ?? string.Empty),

            new("rsatimestamp", rsaResponse.Timestamp),
            new("remember_login", "true"),
            new("oauth_client_id", "DE45CD61"),
            new("oauth_scope", "read_profile write_profile read_client write_client"),
        };

        StringBuilder stringBuilder = new StringBuilder(50);
        stringBuilder.Append($"sessionid={data.SessionCookie};");
        if (data.LoginSecure is not null)
            stringBuilder.Append($"steamLoginSecure={data.LoginSecure};");

        if (await _legacySteamCommunityApi.DoLogin(postData, stringBuilder.ToString(), cancellationToken) is not { } loginResponse)
        {
            return new LoginAgainData(data.CaptchaGid is not null
                ? LoginResult.TooManyFailedLogins
                : LoginResult.BadCredentials);
        }

        data.LoginSecure = loginResponse.LoginSecure;
        data.SteamId = loginResponse.EmailSteamId;

        if (loginResponse.CaptchaNeeded)
        {
            data.LoginResult =  LoginResult.NeedCaptcha;
            data.CaptchaGid = loginResponse.CaptchaGid?.Deserialize<string>();

            return data;
        }

        if (loginResponse.EmailAuthNeeded)
        {
            data.LoginResult = LoginResult.NeedEmail;
            return data;
        }

        if (loginResponse.TwoFactorNeeded)
        {
            data.LoginResult = LoginResult.Need2Fa;
            return await LoginAgain(account, password, data, cancellationToken);
        }

        if (loginResponse is { Success: true, TransferParameters: not null })
        {
            var transferParameters = loginResponse.TransferParameters;
            
            account.Session = new SessionData()
            {
                WebCookie = transferParameters.Webcookie,
                SteamId = UInt64.Parse(transferParameters.Steamid),
                SteamLoginSecure = loginResponse.LoginSecure,
                SessionId = data.SessionCookie,
            };

            return new LoginAgainData(LoginResult.LoginOkay);
        }

        return new LoginAgainData(LoginResult.GeneralFailure);
    }

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

    private static string EncryptPassword(string password, RsaResponse rsaResponse)
    {
        byte[] encryptedPasswordBytes;
        using (var rsaEncryptor = new RSACryptoServiceProvider())
        {
            var passwordBytes = Encoding.ASCII.GetBytes(password);
            var rsaParameters = rsaEncryptor.ExportParameters(false);
            rsaParameters.Exponent = Util.HexStringToByteArray(rsaResponse.Exponent);
            rsaParameters.Modulus = Util.HexStringToByteArray(rsaResponse.Modulus);
            rsaEncryptor.ImportParameters(rsaParameters);
            encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);
        }

        return Convert.ToBase64String(encryptedPasswordBytes);
    }
}
