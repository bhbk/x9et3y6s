using System;

namespace Bhbk.Lib.Identity.Primitives
{
    public class Enums
    {
        //https://tools.ietf.org/html/rfc6749#section-2.1
        public enum AudienceType
        {
            user_agent,
            native,
            server,
        }

        public enum GrantType
        {
            access_token,
            authorize_code,
            client_credential,
            refresh_token,
        }

        public enum LoginType
        {
            GenerateAccessTokenV1,
            GenerateAccessTokenV1CompatibilityMode,
            GenerateAccessTokenV2,
            GenerateRefreshTokenV1,
            GenerateRefreshTokenV2,
            GenerateAuthorizationCodeV1,
            GenerateAuthorizationCodeV2,
        }

        public enum TaskType
        {
            MaintainActivity,
            MaintainQuotes,
            MaintainTokens,
            MaintainUsers,
        }
    }
}
