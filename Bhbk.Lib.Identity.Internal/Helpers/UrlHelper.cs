using Bhbk.Lib.Identity.Internal.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Internal.Helpers
{
    public class UrlHelper
    {
        public static Uri GenerateConfirmEmail(IConfigurationRoot conf, tbl_Users user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-email");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri GenerateConfirmPassword(IConfigurationRoot conf, tbl_Users user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-password");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri GenerateConfirmPhone(IConfigurationRoot conf, tbl_Users user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-phone");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }
    }
}
