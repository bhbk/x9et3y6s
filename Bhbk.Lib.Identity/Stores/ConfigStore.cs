using Bhbk.Lib.Identity.Factory;
using Microsoft.Extensions.Configuration;
using Bhbk.Lib.Identity.Helpers;
using System;
using System.IO;
using System.Reflection;

namespace Bhbk.Lib.Identity.Stores
{
    public class ConfigStore
    {
        private ConfigModel _context { get; set; }

        public ConfigModel Config
        {
            get
            {
                return _context;
            }
        }

        public void ReadJson()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(FileHelper.FindFileInDefaultPaths("globalsettings.json").DirectoryName)
                .AddJsonFile("globalsettings.json")
                .Build();

            _context = new ConfigModel();

            try
            {
                _context.Debug = Boolean.Parse(config["Debug"]);
                _context.DefaultAuhthorizationCodeLife = UInt16.Parse(config["DefaultAuthorizationCodeLife"]);
                _context.DefaultAuhthorizationCodeLength = UInt16.Parse(config["DefaultAuthorizationCodeLength"]);
                _context.DefaultAccessTokenLife = Double.Parse(config["DefaultAccessTokenLife"]);
                _context.DefaultFailedAccessAttempts = UInt16.Parse(config["DefaultFailedAccessAttempts"]);
                _context.DefaultPasswordLength = UInt16.Parse(config["DefaultPasswordLength"]);
                _context.DefaultRefreshTokenLife = Double.Parse(config["DefaultRefreshTokenLife"]);
                _context.IdentityAdminBaseUrl = config["IdentityAdminBaseUrl"];
                _context.IdentityMeBaseUrl = config["IdentityMeBaseUrl"];
                _context.IdentityStsBaseUrl = config["IdentityStsBaseUrl"];
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
