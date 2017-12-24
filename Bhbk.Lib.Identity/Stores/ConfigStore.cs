using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Bhbk.Lib.Identity.Stores
{
    public class ConfigStore
    {
        private AppConfig _context { get; set; }
        private FileInfo _cf = FileSystemHelper.SearchPaths("globalsettings.json");
        private IConfigurationRoot _cb;

        public AppConfig Config
        {
            get
            {
                return _context;
            }
        }

        public void ReadJson()
        {
            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name)
                .Build();

            _context = new AppConfig();

            try
            {
                _context.Debug = Boolean.Parse(_cb["Debug"]);
                _context.DefaultAuhthorizationCodeLife = UInt16.Parse(_cb["DefaultAuthorizationCodeLife"]);
                _context.DefaultAuhthorizationCodeLength = UInt16.Parse(_cb["DefaultAuthorizationCodeLength"]);
                _context.DefaultAccessTokenLife = Double.Parse(_cb["DefaultAccessTokenLife"]);
                _context.DefaultFailedAccessAttempts = UInt16.Parse(_cb["DefaultFailedAccessAttempts"]);
                _context.DefaultPasswordLength = UInt16.Parse(_cb["DefaultPasswordLength"]);
                _context.DefaultRefreshTokenLife = Double.Parse(_cb["DefaultRefreshTokenLife"]);
                _context.IdentityAdminBaseUrl = _cb["IdentityAdminBaseUrl"];
                _context.IdentityMeBaseUrl = _cb["IdentityMeBaseUrl"];
                _context.IdentityStsBaseUrl = _cb["IdentityStsBaseUrl"];
                _context.UnitTestAccessToken = false;
                _context.UnitTestAccessTokenFakeUtcNow = DateTime.UtcNow;
                _context.UnitTestRefreshToken = false;
                _context.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;
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
