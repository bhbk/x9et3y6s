using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Text;

namespace Bhbk.Lib.Identity.Managers
{
    public class ConfigManager
    {
        private readonly ConfigStore _store;

        public AppConfig Tweaks
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
            output.Append("\tDefaults:Debug:" + _store.Config.DefaultsDebug.ToString() + Environment.NewLine);
            output.Append("\tDefaults:AuthorizationCodeLife:" + _store.Config.DefaultsAuhthorizationCodeLife.ToString() + Environment.NewLine);
            output.Append("\tDefaults:AuthorizationCodeLength:" + _store.Config.DefaultsAuhthorizationCodeLength.ToString() + Environment.NewLine);
            output.Append("\tDefaults:AccessTokenLife:" + _store.Config.DefaultsAccessTokenLife.ToString() + Environment.NewLine);
            output.Append("\tDefaults:FailedAccessAttempts:" + _store.Config.DefaultsFailedAccessAttempts.ToString() + Environment.NewLine);
            output.Append("\tDefaults:PasswordLength:" + _store.Config.DefaultsPasswordLength.ToString() + Environment.NewLine);
            output.Append("\tDefaults:RefreshTokenLife:" + _store.Config.DefaultsRefreshTokenLife.ToString() + Environment.NewLine);
            output.Append("\tEndpoints:AdminUrl:" + _store.Config.EndpointsAdminUrl.ToString() + Environment.NewLine);
            output.Append("\tEndpoints:MeUrl:" + _store.Config.EndpointsMeUrl.ToString() + Environment.NewLine);
            output.Append("\tEndpoints:StsUrl:" + _store.Config.EndpointsStsUrl.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:AccessToken:" + _store.Config.UnitTestsAccessToken.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:AccessTokenFakeUtcNow:" + _store.Config.UnitTestsAccessTokenFakeUtcNow.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:RefreshToken:" + _store.Config.UnitTestsRefreshToken.ToString() + Environment.NewLine);
            output.Append("\tUnitTests:RefreshTokenFakeUtcNow:" + _store.Config.UnitTestsRefreshTokenFakeUtcNow.ToString() + Environment.NewLine);

            return output.ToString();
        }
    }
}
