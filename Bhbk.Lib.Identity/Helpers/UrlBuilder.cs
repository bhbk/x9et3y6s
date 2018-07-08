﻿using Bhbk.Lib.Identity.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Helpers
{
    public class UrlBuilder
    {
        public static Uri UiConfirmEmail(IConfigurationRoot conf, AppUser user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentitySpas:MeUrl"], conf["IdentitySpas:MePath"], "/confirm-email");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri UiConfirmPassword(IConfigurationRoot conf, AppUser user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentitySpas:MeUrl"], conf["IdentitySpas:MePath"], "/confirm-password");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri UiConfirmPhone(IConfigurationRoot conf, AppUser user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentitySpas:MeUrl"], conf["IdentitySpas:MePath"], "/confirm-phone");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri UiAuthorizationCodeRequest(IConfigurationRoot conf, AppClient client, AppUser user, string redirectUri, string scope, string state)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentitySpas:MeUrl"], conf["IdentitySpas:MePath"], "/authorization-code");

            return new Uri(path + "?client=" + client.Id.ToString()
                + "&user=" + user.Id.ToString()
                + "&response_type=" + "code"
                + "&redirect_uri=" + redirectUri
                + "&scope=" + scope
                + "&state=" + state);
        }
    }
}
