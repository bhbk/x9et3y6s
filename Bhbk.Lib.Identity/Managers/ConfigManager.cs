using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Text;

namespace Bhbk.Lib.Identity.Managers
{
    public class ConfigManager
    {
        private ConfigStore _store;

        public ConfigModel Tweaks
        {
            get
            {
                return _store.Config;
            }
        }

        public ConfigManager()
        {
            _store = new ConfigStore();
            _store.ReadJson();
        }

        public ConfigManager(ConfigStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            _store = store;
            _store.ReadJson();
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            output.Append(typeof(ConfigModel).Name + Environment.NewLine);
            output.Append("  Debug:" + _store.Config.Debug.ToString() + Environment.NewLine);
            output.Append("  DefaultAuthorizationCodeLife:" + _store.Config.DefaultAuhthorizationCodeLife.ToString() + Environment.NewLine);
            output.Append("  DefaultAuthorizationCodeLength:" + _store.Config.DefaultAuhthorizationCodeLength.ToString() + Environment.NewLine);
            output.Append("  DefaultAccessTokenLife:" + _store.Config.DefaultAccessTokenLife.ToString() + Environment.NewLine);
            output.Append("  DefaultFailedAccessAttempts:" + _store.Config.DefaultFailedAccessAttempts.ToString() + Environment.NewLine);
            output.Append("  DefaultPasswordLength:" + _store.Config.DefaultPasswordLength.ToString() + Environment.NewLine);
            output.Append("  DefaultRefreshTokenLife:" + _store.Config.DefaultRefreshTokenLife.ToString() + Environment.NewLine);
            output.Append("  IdentityAdminBaseUrl:" + _store.Config.IdentityAdminBaseUrl.ToString() + Environment.NewLine);
            output.Append("  IdentityMeBaseUrl:" + _store.Config.IdentityMeBaseUrl.ToString() + Environment.NewLine);
            output.Append("  IdentityStsBaseUrl:" + _store.Config.IdentityStsBaseUrl.ToString() + Environment.NewLine);
            output.Append("  UnitTestAccessToken:" + _store.Config.UnitTestAccessToken.ToString() + Environment.NewLine);
            output.Append("  UnitTestAccessTokenFakeUtcNow:" + _store.Config.UnitTestAccessTokenFakeUtcNow.ToString() + Environment.NewLine);
            output.Append("  UnitTestRefreshToken:" + _store.Config.UnitTestRefreshToken.ToString() + Environment.NewLine);
            output.Append("  UnitTestRefreshTokenFakeUtcNow:" + _store.Config.UnitTestRefreshTokenFakeUtcNow.ToString() + Environment.NewLine);

            return output.ToString();
        }
    }
}
