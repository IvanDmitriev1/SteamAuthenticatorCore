namespace SteamAuthenticatorCore.Maui.Extensions;

internal static class SteamGuardAccountExtensions
{
    public static SteamGuardAccount MapFromDto(this SteamGuardAccountDto dto, SessionDataDto sessionDataDto)
    {
        return new SteamGuardAccount()
        {
            SharedSecret = dto.SharedSecret,
            SerialNumber = dto.SerialNumber,
            Uri = dto.Uri,
            RevocationCode = dto.RevocationCode,
            AccountName = dto.AccountName,
            IdentitySecret = dto.IdentitySecret,
            Secret1 = dto.Secret1,
            DeviceId = dto.DeviceId,
            Session = new SessionData()
            {
                AccessToken = sessionDataDto.AccessToken,
                SteamId = (ulong) sessionDataDto.SteamId,
                SessionId = sessionDataDto.SessionId,
                WebCookie = sessionDataDto.WebCookie,
                SteamLoginSecure = sessionDataDto.SteamLoginSecure
            }
        };
    }

    public static SteamGuardAccountDto MapToDto(this SteamGuardAccount account, int sessionId)
    {
        var dto = new SteamGuardAccountDto
        {
            SharedSecret = account.SharedSecret,
            SerialNumber = account.SerialNumber,
            Uri = account.Uri,
            RevocationCode = account.RevocationCode,
            AccountName = account.AccountName,
            IdentitySecret = account.IdentitySecret,
            Secret1 = account.Secret1,
            DeviceId = account.DeviceId,
            SessionId = sessionId
        };

        return dto;
    }

    public static SessionDataDto MapToDto(this SessionData sessionData)
    {
        return new SessionDataDto()
        {
            SessionId = sessionData.SessionId,
            SteamLoginSecure = sessionData.SteamLoginSecure,
            WebCookie = sessionData.WebCookie,
            AccessToken = sessionData.AccessToken,
            SteamId = (long)sessionData.SteamId
        };
    }
}