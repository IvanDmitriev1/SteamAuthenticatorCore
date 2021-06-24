using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuthCore.Models;

namespace SteamAuthCore
{
    /// <summary>
    /// Class to help align system time with the Steam server time. Not super advanced; probably not taking some things into account that it should.
    /// Necessary to generate up-to-date codes. In general, this will have an error of less than a second, assuming Steam is operational.
    /// </summary>
    public class TimeAligner
    {
        #region HelpClass

        internal class TimeQuery
        {
            internal class TimeQueryResponse
            {
                [JsonProperty("server_time")]
                public string ServerTime { get; set; } = null!;
            }

            [JsonProperty("response")]
            public TimeQueryResponse Response { get; set; } = null!;
        }

        #endregion

        #region Variabels

        private static bool _aligned = false;
        private static int _timeDifference = 0;

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

        public static void AlignTime()
        {
            using WebClient client = new();
            try
            {
                string response = client.UploadString(ApiEndpoints.TwoFactorTimeQuery, "steamid=0");
                TimeAligning(response, Util.GetSystemUnixTime());
            }
            catch (WebException)
            {
                return;
            }
        }

        public static async Task AlignTimeAsync()
        {
            WebClient client = new();
            try
            {
                string response = await client.UploadStringTaskAsync(new Uri(ApiEndpoints.TwoFactorTimeQuery), "steamid=0");
                TimeAligning(response, Util.GetSystemUnixTime());
            }
            catch (WebException)
            {
                return;
            }
        }

        private static void TimeAligning(in string response, in Int64 currentTime)
        {
            if (JsonConvert.DeserializeObject<TimeQuery>(response) is not { } query)
                throw new ArgumentNullException(nameof(query));

            _timeDifference = (int)(Int64.Parse(query.Response.ServerTime) - currentTime);
            _aligned = true;
        }
    }
}
