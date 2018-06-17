using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Bhbk.Lib.Identity.Stores
{
    public class ConfigStore
    {
        private AppConfig _config { get; set; }
        private FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-lib.json");
        private IConfigurationRoot _cb;

        public AppConfig Config
        {
            get
            {
                return _config;
            }
        }

        public void ReadJson()
        {
            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name)
                .Build();

            _config = new AppConfig();

            try
            {
                _config.DefaultsDebug = Boolean.Parse(_cb["IdentityDefaults:Debug"]);
                _config.DefaultsAuhthorizationCodeLife = UInt16.Parse(_cb["IdentityDefaults:AuthorizationCodeLife"]);
                _config.DefaultsAuhthorizationCodeLength = UInt16.Parse(_cb["IdentityDefaults:AuthorizationCodeLength"]);
                _config.DefaultsAccessTokenLife = Double.Parse(_cb["IdentityDefaults:AccessTokenLife"]);
                _config.DefaultsFailedAccessAttempts = UInt16.Parse(_cb["IdentityDefaults:FailedAccessAttempts"]);
                _config.DefaultsPasswordLength = UInt16.Parse(_cb["IdentityDefaults:PasswordLength"]);
                _config.DefaultsRefreshTokenLife = Double.Parse(_cb["IdentityDefaults:RefreshTokenLife"]);
                _config.EndpointsAdminUrl = _cb["IdentityEndpoints:AdminUrl"];
                _config.EndpointsMeUrl = _cb["IdentityEndpoints:MeUrl"];
                _config.EndpointsStsUrl = _cb["IdentityEndpoints:StsUrl"];
                _config.UnitTestsAccessToken = false;
                _config.UnitTestsAccessTokenFakeUtcNow = DateTime.UtcNow;
                _config.UnitTestsRefreshToken = false;
                _config.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                throw new ArgumentNullException();
            }
        }

        public void WriteJson()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                throw new ArgumentNullException();
            }
        }
    }
}
