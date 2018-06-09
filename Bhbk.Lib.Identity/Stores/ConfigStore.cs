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
        private FileInfo _cf = FileSystemHelper.SearchPaths("globalsettings.json");
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
                _config.Debug = Boolean.Parse(_cb["Debug"]);
                _config.DefaultAuhthorizationCodeLife = UInt16.Parse(_cb["DefaultAuthorizationCodeLife"]);
                _config.DefaultAuhthorizationCodeLength = UInt16.Parse(_cb["DefaultAuthorizationCodeLength"]);
                _config.DefaultAccessTokenLife = Double.Parse(_cb["DefaultAccessTokenLife"]);
                _config.DefaultFailedAccessAttempts = UInt16.Parse(_cb["DefaultFailedAccessAttempts"]);
                _config.DefaultPasswordLength = UInt16.Parse(_cb["DefaultPasswordLength"]);
                _config.DefaultRefreshTokenLife = Double.Parse(_cb["DefaultRefreshTokenLife"]);
                _config.IdentityAdminBaseUrl = _cb["IdentityAdminBaseUrl"];
                _config.IdentityMeBaseUrl = _cb["IdentityMeBaseUrl"];
                _config.IdentityStsBaseUrl = _cb["IdentityStsBaseUrl"];
                _config.UnitTestAccessToken = false;
                _config.UnitTestAccessTokenFakeUtcNow = DateTime.UtcNow;
                _config.UnitTestRefreshToken = false;
                _config.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;
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
