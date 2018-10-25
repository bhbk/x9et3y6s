﻿using System;

namespace Bhbk.Lib.Identity.Factory
{
    public class AppConfig
    {
        public bool DefaultsDebug { get; set; }
        public UInt32 DefaultsAccessTokenExpire { get; set; }
        public UInt32 DefaultsAuthorizationCodeExpire { get; set; }
        public UInt32 DefaultsBrowserCookieExpire { get; set; }
        public UInt32 DefaultsRefreshTokenExpire { get; set; }
        public bool DefaultsCompatibilityModeClaims { get; set; }
        public bool DefaultsCompatibilityModeIssuer { get; set; }
        public bool UnitTestsAccessToken { get; set; }
        public DateTime UnitTestsAccessTokenFakeUtcNow { get; set; }
        public bool UnitTestsRefreshToken { get; set; }
        public DateTime UnitTestsRefreshTokenFakeUtcNow { get; set; }
    }
}
