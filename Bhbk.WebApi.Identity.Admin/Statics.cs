using System;
using System.Configuration;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Statics
    {
        #region Configuration Settings
        #endregion

        #region S2S/Credential Settings
        public static readonly string ApiIdentityAdminBaseUrl = ConfigurationManager.AppSettings["IdentityAdminBaseUrl"];
        public static readonly string ApiIdentityMeBaseUrl = ConfigurationManager.AppSettings["IdentityMeBaseUrl"];
        #endregion
    }
}