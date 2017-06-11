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
            output.Append("  DefaultPasswordLength:" + _store.Config.DefaultPasswordLength.ToString() + Environment.NewLine);
            output.Append("  DefaultAuthorizationCodeLife:" + _store.Config.DefaultAuthorizationCodeLife.ToString() + Environment.NewLine);
            output.Append("  DefaultAccessTokenLife:" + _store.Config.DefaultAccessTokenLife.ToString() + Environment.NewLine);
            output.Append("  DefaultRefreshTokenLife:" + _store.Config.DefaultRefreshTokenLife.ToString() + Environment.NewLine);
            output.Append("  IdentityAdminBaseUrl:" + _store.Config.IdentityAdminBaseUrl.ToString() + Environment.NewLine);
            output.Append("  IdentityMeBaseUrl:" + _store.Config.IdentityMeBaseUrl.ToString() + Environment.NewLine);
            output.Append("  IdentityStsBaseUrl:" + _store.Config.IdentityStsBaseUrl.ToString() + Environment.NewLine);
            output.Append("  UnitTestAccessToken:" + _store.Config.UnitTestAccessToken.ToString() + Environment.NewLine);
            output.Append("  UnitTestAccessTokenFakeUtcNow:" + _store.Config.UnitTestAccessTokenFakeUtcNow.ToString() + Environment.NewLine);
            output.Append("  UnitTestRefreshToken:" + _store.Config.UnitTestRefreshToken.ToString() + Environment.NewLine);
            output.Append("  UnitTestRefreshTokenFakeUtcNow:" + _store.Config.UnitTestRefreshTokenFakeUtcNow.ToString() + Environment.NewLine);
            output.Append("  UnitTestRun:" + _store.Config.UnitTestRun.ToString() + Environment.NewLine);

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
