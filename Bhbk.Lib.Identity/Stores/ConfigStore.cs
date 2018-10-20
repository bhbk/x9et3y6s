using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Factory;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Bhbk.Lib.Identity.Stores
{
    public class ConfigStore
    {
        private FileInfo _lib = Search.DefaultPaths("appsettings-lib.json");
        private AppConfig _app { get; set; }
        private IConfigurationRoot _conf;

        public AppConfig Config
        {
            get
            {
                return _app;
            }
        }

        public void ReadJson()
        {
            _conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name)
                .Build();

            _app = new AppConfig();

            try
            {
                _app.DefaultsAccessTokenExpire = UInt32.Parse(_conf["IdentityDefaults:AccessTokenExpire"]);
                _app.DefaultsAuthorizationCodeExpire = UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"]);
                _app.DefaultsBrowserCookieExpire = UInt32.Parse(_conf["IdentityDefaults:BrowserCookieExpire"]);
                _app.DefaultsRefreshTokenExpire = UInt32.Parse(_conf["IdentityDefaults:RefreshTokenExpire"]);
                _app.UnitTestsAccessToken = false;
                _app.UnitTestsAccessTokenFakeUtcNow = DateTime.UtcNow;
                _app.UnitTestsRefreshToken = false;
                _app.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;
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
