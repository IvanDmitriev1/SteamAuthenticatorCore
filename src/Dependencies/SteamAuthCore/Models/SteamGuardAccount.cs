using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SteamAuthCore.Abstractions;

namespace SteamAuthCore.Models;

public class SteamGuardAccount
{
    #region Properties

    [JsonPropertyName("shared_secret")]
    public string? SharedSecret { get; set; }

    [JsonPropertyName("serial_number")]
    public string SerialNumber { get; set; } = null!;

    [JsonPropertyName("uri")]
    public string Uri { get; set; } = null!;

    [JsonPropertyName("revocation_code")]
    public string RevocationCode { get; set; } = null!;

    [JsonPropertyName("account_name")]
    public string AccountName { get; set; } = null!;

    [JsonPropertyName("identity_secret")]
    public string IdentitySecret { get; set; } = null!;

    [JsonPropertyName("secret_1")]
    public string Secret1 { get; set; } = null!;

    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = null!;

    public SessionData Session { get; set; } = null!;

    #endregion

    private static readonly byte[] SteamGuardCodeTranslations = { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create();

    public string? GenerateSteamGuardCode()
    {
        if (string.IsNullOrEmpty(SharedSecret))
            return null;

        var timeArray = ArrayPool.Rent(8);

        try
        {
            using var hmacGenerator = new HMACSHA1(Convert.FromBase64String(Regex.Unescape(SharedSecret!)));

            var time = ITimeAligner.SteamTime;

            time /= 30L;

            for (int i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            var hashedData= hmacGenerator.ComputeHash(timeArray, 0, 8);

            Span<byte> codeArray = stackalloc byte[5];
            try
            {
                byte b = (byte)(hashedData[19] & 0xF);
                int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

                for (int i = 0; i< 5; ++i)
                {
                    codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                    codePoint /= SteamGuardCodeTranslations.Length;
                }
            }
            catch (Exception)
            {
                return null; //Change later, catch-alls are bad!
            }

            return Encoding.UTF8.GetString(codeArray);
        }
        finally
        {
            ArrayPool.Return(timeArray, true);
        }
    }
}