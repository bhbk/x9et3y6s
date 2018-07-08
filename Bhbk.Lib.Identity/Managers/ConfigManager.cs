using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Text;

namespace Bhbk.Lib.Identity.Managers
{
    public class ConfigManager
    {
        private readonly ConfigStore _store;

        public AppConfig Store
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

            output.Append(typeof(AppConfig).Name + Environment.NewLine);
            output.Append("\tDefaults:AccessTokenExpire:" + _store.Config.DefaultsAccessTokenExpire.ToString() + Environment.NewLine);
            output.Append("\tDefaults:AuthorizeCodeExpire:" + _store.Config.DefaultsAuthorizationCodeExpire.ToString() + Environment.NewLine);
            output.Append("\tDefaults:RefreshTokenExpire:" + _store.Config.DefaultsRefreshTokenExpire.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:AccessToken:" + _store.Config.UnitTestsAccessToken.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:AccessTokenFakeUtcNow:" + _store.Config.UnitTestsAccessTokenFakeUtcNow.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:RefreshToken:" + _store.Config.UnitTestsRefreshToken.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:RefreshTokenFakeUtcNow:" + _store.Config.UnitTestsRefreshTokenFakeUtcNow.ToString() + Environment.NewLine);

            return output.ToString();
        }
    }
}
