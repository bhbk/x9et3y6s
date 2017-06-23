using Bhbk.Lib.Identity.Infrastructure;
using System;
using System.Xml;

namespace Bhbk.Lib.Identity.Store
{
    public class ConfigStore
    {
        private ConfigModel _context;

        public ConfigModel Config
        {
            get
            {
                return _context;
            }
        }

        public void ReadXml(XmlReader reader)
        {
            _context = new ConfigModel();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("/Global");

                _context.Debug = Boolean.Parse(node.SelectSingleNode("Debug").InnerText);
                _context.DefaultAuhthorizationCodeLife = UInt16.Parse(node.SelectSingleNode("DefaultAuthorizationCodeLife").InnerText);
                _context.DefaultAuhthorizationCodeLength = UInt16.Parse(node.SelectSingleNode("DefaultAuthorizationCodeLength").InnerText);
                _context.DefaultAccessTokenLife = Double.Parse(node.SelectSingleNode("DefaultAccessTokenLife").InnerText);
                _context.DefaultFailedAccessAttempts = UInt16.Parse(node.SelectSingleNode("DefaultFailedAccessAttempts").InnerText);
                _context.DefaultPasswordLength = UInt16.Parse(node.SelectSingleNode("DefaultPasswordLength").InnerText);
                _context.DefaultRefreshTokenLife = Double.Parse(node.SelectSingleNode("DefaultRefreshTokenLife").InnerText);
                _context.IdentityAdminBaseUrl = node.SelectSingleNode("IdentityAdminBaseUrl").InnerText;
                _context.IdentityMeBaseUrl = node.SelectSingleNode("IdentityMeBaseUrl").InnerText;
                _context.IdentityStsBaseUrl = node.SelectSingleNode("IdentityStsBaseUrl").InnerText;
                _context.UnitTestAccessToken = false;
                _context.UnitTestAccessTokenFakeUtcNow = DateTime.UtcNow;
                _context.UnitTestRefreshToken = false;
                _context.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;
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
