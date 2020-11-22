using Microsoft.Extensions.Configuration;
using System;
using System.Web;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public class UrlFactory
    {
        public static Uri GenerateAuthCodeV2(Uri authorize, Uri redirect, string issuer, string audience, string user, string state)
        {
            return new Uri(authorize.AbsoluteUri + "?issuer=" + HttpUtility.UrlEncode(issuer)
                + "&client=" + HttpUtility.UrlEncode(audience)
                + "&user=" + HttpUtility.UrlEncode(user)
                + "&response_type=code"
                + "&redirect_uri=" + HttpUtility.UrlEncode(redirect.AbsoluteUri)
                + "&state=" + HttpUtility.UrlEncode(state));
        }

        public static Uri GenerateConfirmEmailV1(IConfiguration conf, string user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-email");

            return new Uri(path + "?user=" + user
                + "&code=" + code);
        }

        public static Uri GenerateConfirmPasswordV1(IConfiguration conf, string user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-password");

            return new Uri(path + "?user=" + user
                + "&code=" + code);
        }

        public static Uri GenerateConfirmPhoneV1(IConfiguration conf, string user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-phone");

            return new Uri(path + "?user=" + user
                + "&code=" + code);
        }
    }
}
