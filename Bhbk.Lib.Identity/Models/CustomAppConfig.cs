using System;

namespace Bhbk.Lib.Identity.Factory
{
    public class AppConfig
    {
        public bool DefaultsDebug { get; set; }
        public Double DefaultsAccessTokenLife { get; set; }
        public UInt16 DefaultsAuhthorizationCodeLife { get; set; }
        public UInt16 DefaultsAuhthorizationCodeLength { get; set; }
        public UInt16 DefaultsPasswordLength { get; set; }
        public UInt16 DefaultsFailedAccessAttempts { get; set; }
        public Double DefaultsRefreshTokenLife { get; set; }
        public string EndpointsAdminBaseUrl { get; set; }
        public string EndpointsMeBaseUrl { get; set; }
        public string EndpointsStsBaseUrl { get; set; }
        public bool UnitTestsAccessToken { get; set; }
        public DateTime UnitTestsAccessTokenFakeUtcNow { get; set; }
        public bool UnitTestsRefreshToken { get; set; }
        public DateTime UnitTestsRefreshTokenFakeUtcNow { get; set; }
    }
}
