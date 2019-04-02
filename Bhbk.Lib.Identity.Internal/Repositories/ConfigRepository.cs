using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class ConfigRepository
    {
        private readonly ExecutionType _situation;
        private UInt32 _defaultsExpireAuthCodeRefresh;
        private UInt32 _defaultsExpireAuthCodeToken;
        private UInt32 _defaultsExpireAuthCodeTOTP;
        private UInt32 _defaultsExpireClientRefresh;
        private UInt32 _defaultsExpireClientToken;
        private UInt32 _defaultsExpireDeviceCodeRefresh;
        private UInt32 _defaultsExpireDeviceCodeToken;
        private UInt32 _defaultsExpireDeviceCodeTOTP;
        private UInt32 _defaultsExpirePasswordRefresh;
        private UInt32 _defaultsExpirePasswordToken;
        private bool _defaultsLegacyModeClaims;
        private bool _defaultsLegacyModeIssuer;
        private bool _unitTestsPasswordToken;
        private bool _unitTestsRefreshToken;
        private DateTime _unitTestsPasswordTokenFakeUtcNow;
        private DateTime _unitTestsRefreshTokenFakeUtcNow;

        public ConfigRepository(IConfigurationRoot conf, ExecutionType situation)
        {
            _situation = situation;

            _defaultsExpireAuthCodeRefresh = UInt32.Parse(conf["IdentityDefaults:ExpireAuthCodeRefresh"]);
            _defaultsExpireAuthCodeToken = UInt32.Parse(conf["IdentityDefaults:ExpireAuthCodeToken"]);
            _defaultsExpireAuthCodeTOTP = UInt32.Parse(conf["IdentityDefaults:ExpireAuthCodeTOTP"]);

            _defaultsExpireClientRefresh = UInt32.Parse(conf["IdentityDefaults:ExpireClientRefresh"]);
            _defaultsExpireClientToken = UInt32.Parse(conf["IdentityDefaults:ExpireClientToken"]);

            _defaultsExpireDeviceCodeRefresh = UInt32.Parse(conf["IdentityDefaults:ExpireDeviceCodeRefresh"]);
            _defaultsExpireDeviceCodeToken = UInt32.Parse(conf["IdentityDefaults:ExpireDeviceCodeToken"]);
            _defaultsExpireDeviceCodeTOTP = UInt32.Parse(conf["IdentityDefaults:ExpireDeviceCodeTOTP"]);

            _defaultsExpirePasswordRefresh = UInt32.Parse(conf["IdentityDefaults:ExpirePasswordRefresh"]);
            _defaultsExpirePasswordToken = UInt32.Parse(conf["IdentityDefaults:ExpirePasswordToken"]);

            _defaultsLegacyModeClaims = bool.Parse(conf["IdentityDefaults:LegacyModeClaims"]);
            _defaultsLegacyModeIssuer = bool.Parse(conf["IdentityDefaults:LegacyModeIssuer"]);

            _unitTestsPasswordToken = false;
            _unitTestsPasswordTokenFakeUtcNow = DateTime.UtcNow;

            _unitTestsRefreshToken = false;
            _unitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;
        }

        public UInt32 DefaultsExpireAuthCodeRefresh
        {
            get { return _defaultsExpireAuthCodeRefresh; }
        }

        public UInt32 DefaultsExpireAuthCodeToken
        {
            get { return _defaultsExpireAuthCodeToken; }
        }

        public UInt32 DefaultsExpireAuthCodeTOTP
        {
            get { return _defaultsExpireAuthCodeTOTP; }
        }

        public UInt32 DefaultsExpireClientRefresh
        {
            get { return _defaultsExpireClientRefresh; }
        }

        public UInt32 DefaultsExpireClientToken
        {
            get { return _defaultsExpireClientToken; }
        }

        public UInt32 DefaultsExpireDeviceCodeRefresh
        {
            get { return _defaultsExpireDeviceCodeRefresh; }
        }

        public UInt32 DefaultsExpireDeviceCodeToken
        {
            get { return _defaultsExpireDeviceCodeToken; }
        }

        public UInt32 DefaultsExpireDeviceCodeTOTP
        {
            get { return _defaultsExpireDeviceCodeTOTP; }
        }

        public UInt32 DefaultsExpirePasswordRefresh
        {
            get { return _defaultsExpirePasswordRefresh; }
        }

        public UInt32 DefaultsExpirePasswordToken
        {
            get { return _defaultsExpirePasswordToken; }
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

        public bool UnitTestsPasswordRefresh
        {
            get { return _unitTestsRefreshToken; }
            set { _unitTestsRefreshToken = value; }
        }

        public bool UnitTestsPasswordToken
        {
            get { return _unitTestsPasswordToken; }
            set { _unitTestsPasswordToken = value; }
        }

        public DateTime UnitTestsPasswordRefreshFakeUtcNow
        {
            get { return _unitTestsRefreshTokenFakeUtcNow; }
            set { _unitTestsRefreshTokenFakeUtcNow = value; }
        }

        public DateTime UnitTestsPasswordTokenFakeUtcNow
        {
            get { return _unitTestsPasswordTokenFakeUtcNow; }
            set { _unitTestsPasswordTokenFakeUtcNow = value; }
        }
    }
}
