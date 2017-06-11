using Bhbk.Lib.Identity.Infrastructure;
using System;
using System.Xml;

namespace Bhbk.Lib.Identity.Store
{
    public class CustomConfigStore
    {
        private ConfigModel _config;

        public ConfigModel Config
        {
            get
            {
                return _config;
            }
        }

        public void ReadXml(XmlReader reader)
        {
            _config = new ConfigModel();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("/ConfigModel");

                _config.Debug = Boolean.Parse(node.SelectSingleNode("Debug").InnerText);
                _config.DefaultPasswordLength = UInt16.Parse(node.SelectSingleNode("DefaultPasswordLength").InnerText);
                _config.DefaultAuthorizationCodeLife = Double.Parse(node.SelectSingleNode("DefaultAuthorizationCodeLife").InnerText);
                _config.DefaultAccessTokenLife = Double.Parse(node.SelectSingleNode("DefaultAccessTokenLife").InnerText);
                _config.DefaultRefreshTokenLife = Double.Parse(node.SelectSingleNode("DefaultRefreshTokenLife").InnerText);
                _config.IdentityAdminBaseUrl = node.SelectSingleNode("IdentityAdminBaseUrl").InnerText;
                _config.IdentityMeBaseUrl = node.SelectSingleNode("IdentityMeBaseUrl").InnerText;
                _config.IdentityStsBaseUrl = node.SelectSingleNode("IdentityStsBaseUrl").InnerText;
                _config.UnitTestAccessToken = false;
                _config.UnitTestAccessTokenFakeUtcNow = DateTime.UtcNow;
                _config.UnitTestRefreshToken = false;
                _config.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;
                _config.UnitTestRun = false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                throw new XmlException();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }
    }
}
