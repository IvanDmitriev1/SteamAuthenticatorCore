using System;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace SteamAuthCore
{
    public class AuthenticatorLinker
    {
        public AuthenticatorLinker(SessionData session)
        {
            _session = session;
            DeviceId = GenerateDeviceId();

            _cookies = session.GetCookies();
        }

        #region HelpEnums
        public enum LinkResult
        {
            MustProvidePhoneNumber, //No phone number on the account
            MustRemovePhoneNumber, //A phone number is already on the account
            MustConfirmEmail, //User need to click link from confirmation email
            AwaitingFinalization, //Must provide an SMS code
            GeneralFailure, //General failure (really now!)
            AuthenticatorPresent
        }

        public enum FinalizeResult
        {
            BadSmsCode,
            UnableToGenerateCorrectCodes,
            Success,
            GeneralFailure
        }

        #endregion

        #region HelpClasses
        private class AddAuthenticatorResponse
        {
            [JsonPropertyName("response")]
            public SteamGuardAccount? Response { get; set; }
        }

        private class FinalizeAuthenticatorResponse
        {
            [JsonPropertyName("response")]
            public FinalizeAuthenticatorInternalResponse? Response { get; set; }

            internal class FinalizeAuthenticatorInternalResponse
            {
                [JsonPropertyName("status")]
                public int Status { get; set; }

                [JsonPropertyName("server_time")]
                public long ServerTime { get; set; }

                [JsonPropertyName("want_more")]
                public bool WantMore { get; set; }

                [JsonPropertyName("success")]
                public bool Success { get; set; }
            }
        }

        private class HasPhoneResponse
        {
            [JsonPropertyName("has_phone")]
            public bool HasPhone { get; set; }
        }

        private class AddPhoneResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }


        #endregion

        #region Fields

        /// <summary>
        /// Randomly-generated device ID. Should only be generated once per linker.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// After the initial link step, if successful, this will be the SteamGuard data for the account. PLEASE save this somewhere after generating it; it's vital data.
        /// </summary>
        public SteamGuardAccount LinkedAccount { get; private set; } = null!;

        /// <summary>
        /// Set to register a new phone number when linking. If a phone number is not set on the account, this must be set. If a phone number is set on the account, this must be null.
        /// </summary>
        public string? PhoneNumber { get; set; } = null;

        /// <summary>
        /// True if the authenticator has been fully finalized.
        /// </summary>
        public bool Finalized { get; set; } = false;

        #endregion

        #region Variables

        private readonly SessionData _session;
        private readonly CookieContainer _cookies;
        private bool _confirmationEmailSent;

        #endregion

        public LinkResult AddAuthenticator()
        {
            switch (_hasPhoneAttached())
            {
                case true when PhoneNumber != null:
                    return LinkResult.MustRemovePhoneNumber;

                case false when PhoneNumber == null:
                    return LinkResult.MustProvidePhoneNumber;

                case false when _confirmationEmailSent:
                {
                    if (!_checkEmailConfirmation())
                        return LinkResult.GeneralFailure;

                    break;
                }
                case false when !AddPhoneNumber():
                    return LinkResult.GeneralFailure;

                case false:
                    _confirmationEmailSent = true;
                    return LinkResult.MustConfirmEmail;
            }

            NameValueCollection postData = new()
            {
                {"access_token", _session.OAuthToken},
                {"steamid", _session.SteamId.ToString()},
                {"authenticator_type", "1"},
                {"device_identifier", DeviceId},
                {"sms_phone_id", "1"}
            };

            string? response = SteamApi.MobileLoginRequest(ApiEndpoints.SteamApiBase + "/ITwoFactorService/AddAuthenticator/v0001", SteamApi.RequestMethod.Post, postData);
            if (response is null) return LinkResult.GeneralFailure;

            AddAuthenticatorResponse? addAuthenticatorResponse = JsonSerializer.Deserialize<AddAuthenticatorResponse>(response);
            if (addAuthenticatorResponse?.Response is null)
                return LinkResult.GeneralFailure;

            if (addAuthenticatorResponse.Response.Status == 29)
                return LinkResult.AuthenticatorPresent;

            if (addAuthenticatorResponse.Response.Status != 1)
                return LinkResult.GeneralFailure;

            LinkedAccount = addAuthenticatorResponse.Response;
            LinkedAccount.Session = _session;
            LinkedAccount.DeviceId = DeviceId;

            return LinkResult.AwaitingFinalization;
        }

        public FinalizeResult FinalizeAddAuthenticator(string smsCode)
        {
            //The act of checking the SMS code is necessary for Steam to finalize adding the phone number to the account.
            //Of course, we only want to check it if we're adding a phone number in the first place...

            if (!string.IsNullOrEmpty(PhoneNumber) && !CheckSmsCode(smsCode))
            {
                return FinalizeResult.BadSmsCode;
            }

            NameValueCollection postData = new()
            {
                {"steamid", _session.SteamId.ToString()},
                {"access_token", _session.OAuthToken},
                {"activation_code", smsCode}
            };

            int tries = 0;
            while (tries <= 30)
            {
                postData.Set("authenticator_code", LinkedAccount.GenerateSteamGuardCode());
                postData.Set("authenticator_time", TimeAligner.GetSteamTime().ToString());

                if (SteamApi.MobileLoginRequest(ApiEndpoints.SteamApiBase + "/ITwoFactorService/FinalizeAddAuthenticator/v0001", SteamApi.RequestMethod.Post, postData) is not { } response)
                    return FinalizeResult.GeneralFailure;

                if (JsonSerializer.Deserialize<FinalizeAuthenticatorResponse>(response) is not { } finalizeResponse)
                    throw new ArgumentNullException(nameof(finalizeResponse));

                if (finalizeResponse.Response is null)
                    return FinalizeResult.GeneralFailure;

                switch (finalizeResponse.Response.Status)
                {
                    case 89:
                        return FinalizeResult.BadSmsCode;
                    case 88 when tries >= 30:
                        return FinalizeResult.UnableToGenerateCorrectCodes;
                }

                if (!finalizeResponse.Response.Success)
                    return FinalizeResult.GeneralFailure;

                if (finalizeResponse.Response.WantMore)
                {
                    tries++;
                    continue;
                }

                LinkedAccount.FullyEnrolled = true;
                return FinalizeResult.Success;
            }

            return FinalizeResult.GeneralFailure;
        }

        private bool CheckSmsCode(string smsCode)
        {
            NameValueCollection postData = new()
            {
                {"op", "check_sms_code"},
                {"arg", smsCode},
                {"checkfortos", "0"},
                {"skipvoip", "1"},
                {"sessionid", _session.SessionId}
            };

            if (SteamApi.Request(ApiEndpoints.CommunityBase + "/steamguard/phoneajax", SteamApi.RequestMethod.Post, postData, _cookies) is not { } response)
                return false;

            if (JsonSerializer.Deserialize<AddPhoneResponse>(response) is not { } addPhoneNumberResponse)
                throw new ArgumentNullException(nameof(addPhoneNumberResponse));

            if (addPhoneNumberResponse.Success) return true;

            Thread.Sleep(3500); //It seems that Steam needs a few seconds to finalize the phone number on the account.
            return _hasPhoneAttached();
        }

        private bool AddPhoneNumber()
        {
            NameValueCollection postData = new()
            {
                {"op", "add_phone_number"},
                {"arg", PhoneNumber},
                {"sessionid", _session.SessionId}
            };

            if (SteamApi.Request(ApiEndpoints.CommunityBase + "/steamguard/phoneajax", SteamApi.RequestMethod.Post, postData, _cookies) is not { } response)
                return false;

            if (JsonSerializer.Deserialize<AddPhoneResponse>(response) is not { } addPhoneNumberResponse)
                throw new ArgumentNullException(nameof(addPhoneNumberResponse));

            return addPhoneNumberResponse.Success;
        }

        private bool _checkEmailConfirmation()
        {
            NameValueCollection postData = new()
            {
                {"op", "email_confirmation"},
                {"arg", ""},
                {"sessionid", _session.SessionId}
            };

            if (SteamApi.Request(ApiEndpoints.CommunityBase + "/steamguard/phoneajax", SteamApi.RequestMethod.Post, postData, _cookies) is not { } response)
                return false;

            if (JsonSerializer.Deserialize<AddPhoneResponse>(response) is not { } emailConfirmationResponse)
                throw new ArgumentNullException(nameof(emailConfirmationResponse));

            return emailConfirmationResponse.Success;
        }

        private bool _hasPhoneAttached()
        {
            var postData = new NameValueCollection
            {
                {"op", "has_phone"},
                {"arg", "null"},
                {"sessionid", _session.SessionId}
            };

            if (SteamApi.Request(ApiEndpoints.CommunityBase + "/steamguard/phoneajax", SteamApi.RequestMethod.Post, postData, _cookies) is not { } response)
                return false;

            if (JsonSerializer.Deserialize<HasPhoneResponse>(response) is not { } hasPhoneResponse)
                throw new ArgumentException(nameof(hasPhoneResponse));

            return hasPhoneResponse.HasPhone;
        }

        public static string GenerateDeviceId()
        {
            return "android:" + Guid.NewGuid();
        }
    }
}
