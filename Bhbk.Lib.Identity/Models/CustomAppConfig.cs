using System;

namespace Bhbk.Lib.Identity.Factory
{
    public class AppConfig
    {
        public bool Debug { get; set; }
        public Double DefaultAccessTokenLife { get; set; }
        public UInt16 DefaultAuhthorizationCodeLife { get; set; }
        public UInt16 DefaultAuhthorizationCodeLength { get; set; }
        public UInt16 DefaultPasswordLength { get; set; }
        public UInt16 DefaultFailedAccessAttempts { get; set; }
        public Double DefaultRefreshTokenLife { get; set; }
        public string IdentityAdminBaseUrl { get; set; }
        public string IdentityMeBaseUrl { get; set; }
        public string IdentityStsBaseUrl { get; set; }
        public bool UnitTestAccessToken { get; set; }
        public DateTime UnitTestAccessTokenFakeUtcNow { get; set; }
        public bool UnitTestRefreshToken { get; set; }
        public DateTime UnitTestRefreshTokenFakeUtcNow { get; set; }
    }
}
