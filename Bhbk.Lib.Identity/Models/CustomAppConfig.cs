using System;

namespace Bhbk.Lib.Identity.Factory
{
    public class AppConfig
    {
        public bool DefaultsDebug { get; set; }
        public UInt16 DefaultsAccessTokenExpire { get; set; }
        public UInt16 DefaultsAuthorizationCodeExpire { get; set; }
        public UInt16 DefaultsBrowserCookieExpire { get; set; }
        public UInt16 DefaultsRefreshTokenExpire { get; set; }
        public string EndpointsAdminUrl { get; set; }
        public string EndpointsMeUrl { get; set; }
        public string EndpointsStsUrl { get; set; }
        public bool UnitTestsAccessToken { get; set; }
        public DateTime UnitTestsAccessTokenFakeUtcNow { get; set; }
        public bool UnitTestsRefreshToken { get; set; }
        public DateTime UnitTestsRefreshTokenFakeUtcNow { get; set; }
    }
}
