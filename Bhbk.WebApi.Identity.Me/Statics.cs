using System;
using System.Configuration;

namespace Bhbk.WebApi.Identity.Me
{
    public class Statics
    {
        #region S2S/Credential Settings
        public static readonly string ApiIdentityAdminBaseUrl = ConfigurationManager.AppSettings["IdentityAdminBaseUrl"];
        public static readonly string ApiIdentityMeBaseUrl = ConfigurationManager.AppSettings["IdentityMeBaseUrl"];
        #endregion
    }
}