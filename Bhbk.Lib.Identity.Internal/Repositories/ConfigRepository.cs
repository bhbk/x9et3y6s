using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Internal.Repositories
{
    public class ConfigRepository
    {
        private readonly InstanceContext _instance;
        private readonly IConfigurationRoot _conf;
        private bool _legacyModeClaims;
        private bool _legacyModeIssuer;
        private bool _resourceOwnerRefreshFake;
        private bool _resourceOwnerTokenFake;
        private DateTime _resourceOwnerRefreshFakeUtcNow;
        private DateTime _resourceOwnerTokenFakeUtcNow;

        public ConfigRepository(InstanceContext instance, IConfigurationRoot conf)
        {
            _instance = instance;
            _conf = conf;

            _legacyModeClaims = bool.Parse(_conf["IdentityDefaults:LegacyModeClaims"]);
            _legacyModeIssuer = bool.Parse(_conf["IdentityDefaults:LegacyModeIssuer"]);
            _resourceOwnerTokenFake = false;
            _resourceOwnerTokenFakeUtcNow = DateTime.UtcNow;
            _resourceOwnerRefreshFake = false;
            _resourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;
        }

        public uint AuthCodeRefreshExpire => uint.Parse(_conf["IdentityDefaults:AuthCodeRefreshExpire"]);

        public uint AuthCodeTokenExpire => uint.Parse(_conf["IdentityDefaults:AuthCodeTokenExpire"]);

        public uint AuthCodeTotpExpire => uint.Parse(_conf["IdentityDefaults:AuthCodeTotpExpire"]);

        public uint ClientCredRefreshExpire => uint.Parse(_conf["IdentityDefaults:ClientCredRefreshExpire"]);

        public uint ClientCredTokenExpire => uint.Parse(_conf["IdentityDefaults:ClientCredTokenExpire"]);

        public uint DeviceCodePollMax
        {
            get
            {
                if (_instance == InstanceContext.DeployedOrLocal)
                    return uint.Parse(_conf["IdentityDefaults:DeviceCodePollMax"]); ;

                if (_instance == InstanceContext.UnitTest)
                    return 60;

                throw new NotSupportedException();
            }
        }

        public uint DeviceCodeRefreshExpire => uint.Parse(_conf["IdentityDefaults:DeviceCodeRefreshExpire"]);

        public uint DeviceCodeTokenExpire
        {
            get
            {
                if (_instance == InstanceContext.DeployedOrLocal)
                    return uint.Parse(_conf["IdentityDefaults:DeviceCodeTokenExpire"]); ;

                if (_instance == InstanceContext.UnitTest)
                    return 60;

                throw new NotSupportedException();
            }
        }

        public uint DeviceCodeTotpExpire => uint.Parse(_conf["IdentityDefaults:DeviceCodeTotpExpire"]);

        public uint ImplicitTokenExpire
        {
            get
            {
                if (_instance == InstanceContext.DeployedOrLocal)
                    return uint.Parse(_conf["IdentityDefaults:ImplicitTokenExpire"]);

                else if (_instance == InstanceContext.UnitTest)
                    return 60;

                throw new NotSupportedException();
            }
        }

        public bool LegacyModeClaims
        {
            get { return _legacyModeClaims; }
            set { _legacyModeClaims = value; }
        }

        public bool LegacyModeIssuer
        {
            get { return _legacyModeIssuer; }
            set { _legacyModeIssuer = value; }
        }

        public uint ResourceOwnerRefreshExpire => uint.Parse(_conf["IdentityDefaults:ResourceOwnerRefreshExpire"]);

        public uint ResourceOwnerTokenExpire => uint.Parse(_conf["IdentityDefaults:ResourceOwnerTokenExpire"]);

        public bool ResourceOwnerRefreshFake
        {
            get
            {
                if (_instance == InstanceContext.UnitTest)
                    return _resourceOwnerRefreshFake;

                throw new NotSupportedException();
            }
            set
            {
                if (_instance == InstanceContext.UnitTest)
                    _resourceOwnerRefreshFake = value;
            }
        }

        public bool ResourceOwnerTokenFake
        {
            get
            {
                if (_instance == InstanceContext.UnitTest)
                    return _resourceOwnerTokenFake;

                throw new NotSupportedException();
            }
            set
            {
                if (_instance == InstanceContext.UnitTest)
                    _resourceOwnerTokenFake = value;
            }
        }

        public DateTime ResourceOwnerRefreshFakeUtcNow
        {
            get
            {
                if (_instance == InstanceContext.UnitTest)
                    return _resourceOwnerRefreshFakeUtcNow;

                throw new NotSupportedException();
            }
            set
            {
                if (_instance == InstanceContext.UnitTest)
                    _resourceOwnerRefreshFakeUtcNow = value;
            }
        }

        public DateTime ResourceOwnerTokenFakeUtcNow
        {
            get
            {
                if (_instance == InstanceContext.UnitTest)
                    return _resourceOwnerTokenFakeUtcNow;

                throw new NotSupportedException();
            }
            set
            {
                if (_instance == InstanceContext.UnitTest)
                    _resourceOwnerTokenFakeUtcNow = value;
            }
        }
    }
}
