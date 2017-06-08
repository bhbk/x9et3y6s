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
                _config.DefaultPassMinLength = UInt16.Parse(node.SelectSingleNode("DefaultPassMinLength").InnerText);
                _config.DefaultAuthorizationCodeExpire = Double.Parse(node.SelectSingleNode("DefaultAuthorizationCodeExpire").InnerText);
                _config.DefaultTokenExpire = Double.Parse(node.SelectSingleNode("DefaultTokenExpire").InnerText);
                _config.DefaultRefreshTokenExpire = Double.Parse(node.SelectSingleNode("DefaultRefreshTokenExpire").InnerText);
                _config.IdentityAdminBaseUrl = node.SelectSingleNode("IdentityAdminBaseUrl").InnerText;
                _config.IdentityMeBaseUrl = node.SelectSingleNode("IdentityMeBaseUrl").InnerText;
                _config.IdentityStsBaseUrl = node.SelectSingleNode("IdentityStsBaseUrl").InnerText;
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
