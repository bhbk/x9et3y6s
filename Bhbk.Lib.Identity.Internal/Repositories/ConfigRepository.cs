using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class ConfigRepository
    {
        private readonly ExecutionType _situation;
        private UInt32 _defaultsAuthCodeRefreshExpire;
        private UInt32 _defaultsAuthCodeTokenExpire;
        private UInt32 _defaultsAuthCodeTotpExpire;
        private UInt32 _defaultsClientCredRefreshExpire;
        private UInt32 _defaultsClientCredTokenExpire;
        private UInt32 _defaultsDeviceCodePollMax;
        private UInt32 _defaultsDeviceCodeRefreshExpire;
        private UInt32 _defaultsDeviceCodeTokenExpire;
        private UInt32 _defaultsDeviceCodeTotpExpire;
        private bool _defaultsLegacyModeClaims;
        private bool _defaultsLegacyModeIssuer;
        private UInt32 _defaultsResourceOwnerRefreshExpire;
        private UInt32 _defaultsResourceOwnerTokenExpire;
        private UInt32 _unitTestsDeviceCodePollMax;
        private UInt32 _unitTestsDeviceCodeTokenExpire;
        private bool _unitTestsResourceOwnerTokenFake;
        private bool _unitTestsResourceOwnerRefreshFake;
        private DateTime _unitTestsResourceOwnerTokenFakeUtcNow;
        private DateTime _unitTestsResourceOwnerRefreshFakeUtcNow;

        public ConfigRepository(IConfigurationRoot conf, ExecutionType situation)
        {
            _situation = situation;

            _defaultsAuthCodeRefreshExpire = UInt32.Parse(conf["IdentityDefaults:AuthCodeRefreshExpire"]);
            _defaultsAuthCodeTokenExpire = UInt32.Parse(conf["IdentityDefaults:AuthCodeTokenExpire"]);
            _defaultsAuthCodeTotpExpire = UInt32.Parse(conf["IdentityDefaults:AuthCodeTotpExpire"]);

            _defaultsClientCredRefreshExpire = UInt32.Parse(conf["IdentityDefaults:ClientCredRefreshExpire"]);
            _defaultsClientCredTokenExpire = UInt32.Parse(conf["IdentityDefaults:ClientCredTokenExpire"]);

            _defaultsDeviceCodePollMax = UInt32.Parse(conf["IdentityDefaults:DeviceCodePollMax"]);
            _defaultsDeviceCodeRefreshExpire = UInt32.Parse(conf["IdentityDefaults:DeviceCodeRefreshExpire"]);
            _defaultsDeviceCodeTokenExpire = UInt32.Parse(conf["IdentityDefaults:DeviceCodeTokenExpire"]);
            _defaultsDeviceCodeTotpExpire = UInt32.Parse(conf["IdentityDefaults:DeviceCodeTotpExpire"]);

            _defaultsResourceOwnerRefreshExpire = UInt32.Parse(conf["IdentityDefaults:ResourceOwnerRefreshExpire"]);
            _defaultsResourceOwnerTokenExpire = UInt32.Parse(conf["IdentityDefaults:ResourceOwnerTokenExpire"]);

            _defaultsLegacyModeClaims = bool.Parse(conf["IdentityDefaults:LegacyModeClaims"]);
            _defaultsLegacyModeIssuer = bool.Parse(conf["IdentityDefaults:LegacyModeIssuer"]);

            _unitTestsDeviceCodePollMax = 60;
            _unitTestsDeviceCodeTokenExpire = 60;

            _unitTestsResourceOwnerTokenFake = false;
            _unitTestsResourceOwnerTokenFakeUtcNow = DateTime.UtcNow;

            _unitTestsResourceOwnerRefreshFake = false;
            _unitTestsResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;
        }

        public UInt32 DefaultsAuthCodeRefreshExpire
        {
            get { return _defaultsAuthCodeRefreshExpire; }
        }

        public UInt32 DefaultsAuthCodeTokenExpire
        {
            get { return _defaultsAuthCodeTokenExpire; }
        }

        public UInt32 DefaultsAuthCodeTotpExpire
        {
            get { return _defaultsAuthCodeTotpExpire; }
        }

        public UInt32 DefaultsClientCredRefreshExpire
        {
            get { return _defaultsClientCredRefreshExpire; }
        }

        public UInt32 DefaultsClientCredTokenExpire
        {
            get { return _defaultsClientCredTokenExpire; }
        }

        public UInt32 DefaultsDeviceCodePollMax
        {
            get { return _defaultsDeviceCodePollMax; }
        }

        public UInt32 DefaultsDeviceCodeRefreshExpire
        {
            get { return _defaultsDeviceCodeRefreshExpire; }
        }

        public UInt32 DefaultsDeviceCodeTokenExpire
        {
            get { return _defaultsDeviceCodeTokenExpire; }
        }

        public UInt32 DefaultsDeviceCodeTotpExpire
        {
            get { return _defaultsDeviceCodeTotpExpire; }
        }

        public bool DefaultsLegacyModeClaims
        {
            get { return _defaultsLegacyModeClaims; }
            set { _defaultsLegacyModeClaims = value; }
        }

        public bool DefaultsLegacyModeIssuer
        {
            get { return _defaultsLegacyModeIssuer; }
            set { _defaultsLegacyModeIssuer = value; }
        }

        public UInt32 DefaultsResourceOwnerRefreshExpire
        {
            get { return _defaultsResourceOwnerRefreshExpire; }
        }

        public UInt32 DefaultsResourceOwnerTokenExpire
        {
            get { return _defaultsResourceOwnerTokenExpire; }
        }

        public bool UnitTestsResourceOwnerRefreshFake
        {
            get { return _unitTestsResourceOwnerRefreshFake; }
            set { _unitTestsResourceOwnerRefreshFake = value; }
        }

        public bool UnitTestsResourceOwnerTokenFake
        {
            get { return _unitTestsResourceOwnerTokenFake; }
            set { _unitTestsResourceOwnerTokenFake = value; }
        }

        public UInt32 UnitTestsDeviceCodePollMax
        {
            get { return _unitTestsDeviceCodePollMax; }
        }

        public UInt32 UnitTestsDeviceCodeTokenExpire
        {
            get { return _unitTestsDeviceCodeTokenExpire; }
        }

        public DateTime UnitTestsResourceOwnerRefreshFakeUtcNow
        {
            get { return _unitTestsResourceOwnerRefreshFakeUtcNow; }
            set { _unitTestsResourceOwnerRefreshFakeUtcNow = value; }
        }

        public DateTime UnitTestsResourceOwnerTokenFakeUtcNow
        {
            get { return _unitTestsResourceOwnerTokenFakeUtcNow; }
            set { _unitTestsResourceOwnerTokenFakeUtcNow = value; }
        }
    }
}
