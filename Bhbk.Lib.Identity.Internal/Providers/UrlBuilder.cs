using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    public class UrlBuilder
    {
        public static Uri AuthorizationCodeRequest(IConfigurationRoot conf, AppClient client, AppUser user, string redirectUri, string scope, string state)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/authorization-code");

            return new Uri(path + "?client=" + client.Id.ToString()
                + "&user=" + user.Id.ToString()
                + "&response_type=" + "code"
                + "&redirect_uri=" + redirectUri
                + "&scope=" + scope
                + "&state=" + state);
        }

        public static Uri ConfirmEmail(IConfigurationRoot conf, AppUser user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-email");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri ConfirmPassword(IConfigurationRoot conf, AppUser user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-password");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri ConfirmPhone(IConfigurationRoot conf, AppUser user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-phone");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }
    }
}
