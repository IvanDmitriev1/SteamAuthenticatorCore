using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SteamAuthCore
{
    /// <summary>
    /// Class to help align system time with the Steam server time. Not super advanced; probably not taking some things into account that it should.
    /// Necessary to generate up-to-date codes. In general, this will have an error of less than a second, assuming Steam is operational.
    /// </summary>
    public static class TimeAligner
    {
        #region HelpClass

        internal class TimeQuery
        {
            internal class TimeQueryResponse
            {
                [JsonPropertyName("server_time")]
                public string ServerTime { get; set; } = null!;
            }

            [JsonPropertyName("response")]
            public TimeQueryResponse Response { get; set; } = null!;
        }

        #endregion

        #region Variabels

        private static bool _aligned;
        private static Int64 _timeDifference;

        #endregion

        public static long GetSteamTime()
        {
            if (!_aligned)
                AlignTime();

            return Util.GetSystemUnixTime() + _timeDifference;
        }

        public static async Task<Int64> GetSteamTimeAsync()
        {
            if (!_aligned)
                await AlignTimeAsync();

            return Util.GetSystemUnixTime() + _timeDifference;
        }

        private static void AlignTime()
        {
            if (SteamApi.Request<TimeQuery>(ApiEndpoints.TwoFactorTimeQuery, SteamApi.RequestMethod.Post, "steamid=0") is not { } query)
                return;

            _timeDifference = Int64.Parse(query.Response.ServerTime) - Util.GetSystemUnixTime();
            _aligned = true;
        }

        private static async Task AlignTimeAsync()
        {
            if (await SteamApi.RequestAsync<TimeQuery>(ApiEndpoints.TwoFactorTimeQuery, SteamApi.RequestMethod.Post, "steamid=0") is not { } query)
                return;

            _timeDifference = Int64.Parse(query.Response.ServerTime) - Util.GetSystemUnixTime();
            _aligned = true;
        }
    }
}
