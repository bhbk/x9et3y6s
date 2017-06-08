using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Store;
using System;
using System.IO;
using System.Text;
using System.Xml;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Manager
{
    public class CustomConfigManager
    {
        private CustomConfigStore _store;
        private XmlTextReader _xml;

        public ConfigModel Config
        {
            get
            {
                return _store.Config;
            }
        }

        public CustomConfigManager()
        {

            _xml = new XmlTextReader(GetCurrentPath());
            _store = new CustomConfigStore();
            _store.ReadXml(_xml);
        }

        public CustomConfigManager(CustomConfigStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            _xml = new XmlTextReader(GetCurrentPath());
            _store = store;
            _store.ReadXml(_xml);
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            output.Append(typeof(ConfigModel).Name + Environment.NewLine);
            output.Append("  Debug:" + _store.Config.Debug.ToString() + Environment.NewLine);
            output.Append("  DefaultPassMinLength:" + _store.Config.DefaultPassMinLength.ToString() + Environment.NewLine);
            output.Append("  DefaultAuthorizationCodeExpire:" + _store.Config.DefaultAuthorizationCodeExpire.ToString() + Environment.NewLine);
            output.Append("  DefaultTokenExpire:" + _store.Config.DefaultTokenExpire.ToString() + Environment.NewLine);
            output.Append("  DefaultRefreshTokenExpire:" + _store.Config.DefaultRefreshTokenExpire.ToString() + Environment.NewLine);
            output.Append("  IdentityAdminBaseUrl:" + _store.Config.IdentityAdminBaseUrl.ToString() + Environment.NewLine);
            output.Append("  IdentityMeBaseUrl:" + _store.Config.IdentityMeBaseUrl.ToString() + Environment.NewLine);
            output.Append("  IdentityStsBaseUrl:" + _store.Config.IdentityStsBaseUrl.ToString() + Environment.NewLine);

            return output.ToString();
        }

        private string GetCurrentPath()
        {
            string normalPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + BaseLib.Statics.ApiDefaultConfiguration;
            string binPath = AppDomain.CurrentDomain.BaseDirectory + @"\bin\" + BaseLib.Statics.ApiDefaultConfiguration;

            if (File.Exists(normalPath))
                return normalPath;

            else if (File.Exists(binPath))
                return binPath;

            else
                return null;
        }
    }
}
