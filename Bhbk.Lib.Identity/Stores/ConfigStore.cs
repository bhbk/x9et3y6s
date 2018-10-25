using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Factory;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;

namespace Bhbk.Lib.Identity.Stores
{
    public class ConfigStore
    {
        private FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        private IConfigurationRoot _conf;
        private AppConfig _values { get; set; }

        public ConfigStore()
        {
            ReadValues();
        }

        public AppConfig Values
        {
            get
            {
                return _values;
            }
        }

        public void ReadValues()
        {
            _conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name)
                .Build();

            _values = new AppConfig();

            try
            {
                _values.DefaultsAccessTokenExpire = UInt32.Parse(_conf["IdentityDefaults:AccessTokenExpire"]);
                _values.DefaultsAuthorizationCodeExpire = UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"]);
                _values.DefaultsBrowserCookieExpire = UInt32.Parse(_conf["IdentityDefaults:BrowserCookieExpire"]);
                _values.DefaultsRefreshTokenExpire = UInt32.Parse(_conf["IdentityDefaults:RefreshTokenExpire"]);
                _values.DefaultsCompatibilityModeClaims = bool.Parse(_conf["IdentityDefaults:CompatibilityModeClaims"]);
                _values.DefaultsCompatibilityModeIssuer = bool.Parse(_conf["IdentityDefaults:CompatibilityModeIssuer"]);
                _values.UnitTestsAccessToken = false;
                _values.UnitTestsAccessTokenFakeUtcNow = DateTime.UtcNow;
                _values.UnitTestsRefreshToken = false;
                _values.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                throw new ArgumentNullException();
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            output.Append(typeof(AppConfig).Name + Environment.NewLine);
            output.Append("\tDefaults:AccessTokenExpire:" + _values.DefaultsAccessTokenExpire.ToString() + Environment.NewLine);
            output.Append("\tDefaults:AuthorizeCodeExpire:" + _values.DefaultsAuthorizationCodeExpire.ToString() + Environment.NewLine);
            output.Append("\tDefaults:RefreshTokenExpire:" + _values.DefaultsRefreshTokenExpire.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:AccessToken:" + _values.UnitTestsAccessToken.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:AccessTokenFakeUtcNow:" + _values.UnitTestsAccessTokenFakeUtcNow.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:RefreshToken:" + _values.UnitTestsRefreshToken.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:RefreshTokenFakeUtcNow:" + _values.UnitTestsRefreshTokenFakeUtcNow.ToString() + Environment.NewLine);

            return output.ToString();
        }
    }
}
