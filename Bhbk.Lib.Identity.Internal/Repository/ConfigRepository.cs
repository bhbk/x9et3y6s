﻿using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Repository
{
    public class ConfigRepository
    {
        private UInt32 _defaultsAccessTokenExpire;
        private UInt32 _defaultsAuthorizationCodeExpire;
        private UInt32 _defaultsBrowserCookieExpire;
        private UInt32 _defaultsRefreshTokenExpire;
        private bool _defaultsCompatibilityModeClaims;
        private bool _defaultsCompatibilityModeIssuer;
        private bool _unitTestsAccessToken;
        private bool _unitTestsRefreshToken;
        private DateTime _unitTestsAccessTokenfactoryUtcNow;
        private DateTime _unitTestsRefreshTokenfactoryUtcNow;

        public ConfigRepository(IConfigurationRoot conf)
        {
            _defaultsAccessTokenExpire = UInt32.Parse(conf["IdentityDefaults:AccessTokenExpire"]);
            _defaultsAuthorizationCodeExpire = UInt32.Parse(conf["IdentityDefaults:AuthorizationCodeExpire"]);
            _defaultsBrowserCookieExpire = UInt32.Parse(conf["IdentityDefaults:BrowserCookieExpire"]);
            _defaultsRefreshTokenExpire = UInt32.Parse(conf["IdentityDefaults:RefreshTokenExpire"]);
            _defaultsCompatibilityModeClaims = bool.Parse(conf["IdentityDefaults:CompatibilityModeClaims"]);
            _defaultsCompatibilityModeIssuer = bool.Parse(conf["IdentityDefaults:CompatibilityModeIssuer"]);
            _unitTestsAccessToken = false;
            _unitTestsAccessTokenfactoryUtcNow = DateTime.UtcNow;
            _unitTestsRefreshToken = false;
            _unitTestsRefreshTokenfactoryUtcNow = DateTime.UtcNow;
        }

        public UInt32 DefaultsAccessTokenExpire
        {
            get { return _defaultsAccessTokenExpire; }
        }

        public UInt32 DefaultsAuthorizationCodeExpire
        {
            get { return _defaultsAuthorizationCodeExpire; }
        }

        public UInt32 DefaultsBrowserCookieExpire
        {
            get { return _defaultsBrowserCookieExpire; }
        }

        public UInt32 DefaultsRefreshTokenExpire
        {
            get { return _defaultsRefreshTokenExpire; }
        }

        public bool DefaultsCompatibilityModeClaims
        {
            get { return _defaultsCompatibilityModeClaims; }
            set { _defaultsCompatibilityModeClaims = value; }
        }

        public bool DefaultsCompatibilityModeIssuer
        {
            get { return _defaultsCompatibilityModeIssuer; }
            set { _defaultsCompatibilityModeIssuer = value; }
        }

        public bool UnitTestsAccessToken
        {
            get { return _unitTestsAccessToken; }
            set { throw new NotImplementedException(); }
        }

        public bool UnitTestsRefreshToken
        {
            get { return _unitTestsRefreshToken; }
            set { _unitTestsRefreshToken = value; }
        }

        public DateTime UnitTestsAccessTokenfactoryUtcNow
        {
            get { return _unitTestsAccessTokenfactoryUtcNow; }
            set { _unitTestsAccessTokenfactoryUtcNow = value; }
        }

        public DateTime UnitTestsRefreshTokenfactoryUtcNow
        {
            get { return _unitTestsRefreshTokenfactoryUtcNow; }
            set { _unitTestsRefreshTokenfactoryUtcNow = value; }
        }
    }
}
