using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Web;

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public class UrlFactory
    {
        public static Uri GenerateAuthCodeV2(Uri authorize, Uri redirect, tbl_States state)
        {
            return new Uri(authorize.AbsoluteUri + "?issuer=" + HttpUtility.UrlEncode(state.IssuerId.ToString())
                + "&client=" + HttpUtility.UrlEncode(state.AudienceId.ToString())
                + "&user=" + HttpUtility.UrlEncode(state.UserId.ToString())
                + "&response_type=code"
                + "&redirect_uri=" + HttpUtility.UrlEncode(redirect.AbsoluteUri)
                + "&state=" + HttpUtility.UrlEncode(state.StateValue));
        }

        public static Uri GenerateConfirmEmailV1(IConfiguration conf, tbl_Users user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-email");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri GenerateConfirmPasswordV1(IConfiguration conf, tbl_Users user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-password");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }

        public static Uri GenerateConfirmPhoneV1(IConfiguration conf, tbl_Users user, string code)
        {
            var path = string.Format("{0}{1}{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "/confirm-phone");

            return new Uri(path + "?user=" + user.Id.ToString()
                + "&code=" + code);
        }
    }
}
