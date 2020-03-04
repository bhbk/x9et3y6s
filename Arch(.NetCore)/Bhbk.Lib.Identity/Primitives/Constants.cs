using System;

namespace Bhbk.Lib.Identity.Primitives
{
    public class Constants
    {
        #region Attribute Constants

        public const string AttrAudienceIDV1 = "client_id";
        public const string AttrAudienceIDV2 = "client";
        public const string AttrAudienceSecretIDV1 = "client_secret";
        public const string AttrAudienceSecretIDV2 = "client_secret";
        public const string AttrAuthCodeIDV1 = "authorization_code";
        public const string AttrAuthCodeIDV2 = "authorization_code";
        public const string AttrIssuerIDV1 = "issuer_id";
        public const string AttrIssuerIDV2 = "issuer";
        public const string AttrDeviceCodeIDV1 = "device_code";
        public const string AttrDeviceCodeIDV2 = "device_code";
        public const string AttrGrantTypeIDV1 = "grant_type";
        public const string AttrGrantTypeIDV2 = "grant_type";
        public const string AttrRefreshTokenIDV1 = "refresh_token";
        public const string AttrRefreshTokenIDV2 = "refresh_token";
        public const string AttrResourceOwnerIDV1 = "password";
        public const string AttrResourceOwnerIDV2 = "password";
        public const string AttrUserIDV1 = "username";
        public const string AttrUserIDV2 = "user";

        #endregion

        #region Configuration Constants

        public const string ApiDefaultAudienceUi = "Local.Ui";
        public const string ApiDefaultAudienceUiKey = "eBr3r3N1L6JV9jewYJOS6fjZ7EJGeGcb9HUAw6n2ZwY";
        public const string ApiDefaultAudienceApi = "Local.Api";
        public const string ApiDefaultAudienceApiKey = "4Boj4NIFCEZd0D2tyinPVlJ9yzHBl3fV6euTTDO0Xvo";
        public const string ApiDefaultIssuer = "Local";
        public const string ApiDefaultIssuerKey = "8G3zyoTJB4HpL5n3V-htSaN1KFWZGeFgWcUenGZofmw";
        public const string ApiDefaultLogin = "Local";
        public const string ApiDefaultLoginKey = "yUrOgQ1RZefUZ5DnC153sLDy23hmSOwnJ1KHDX61K48";
        public const string ApiDefaultLoginName = "Local";
        public const string ApiDefaultRoleForAdmin = "(Built-In) Admins";
        public const string ApiDefaultRoleForUser = "(Built-In) Users";
        public const string ApiDefaultRoleForService = "(Built-In) Services";
        public const string ApiDefaultAdminUser = "admin@local";
        public const string ApiDefaultAdminUserPassword = "pa$$word01!";
        public const string ApiDefaultAdminUserFirstName = "Administrator";
        public const string ApiDefaultAdminUserLastName = "User";
        public const string ApiDefaultAdminUserPhone = "+00000000000";
        public const string ApiDefaultNormalUser = "user@local";
        public const string ApiDefaultNormalUserPassword = "pa$$word02!";
        public const string ApiDefaultNormalUserFirstName = "Normal";
        public const string ApiDefaultNormalUserLastName = "User";
        public const string ApiDefaultNormalUserPhone = "+00000000000";
        public const string ApiSettingAccessExpire = "AccessExpire";
        public const string ApiSettingRefreshExpire = "RefreshExpire";
        public const string ApiSettingTotpExpire = "TotpExpire";
        public const string ApiSettingPollingMax = "PollingMax";
        public const string ApiSettingGlobalLegacyClaims = "GlobalLegacyClaims";
        public const string ApiSettingGlobalLegacyIssuer = "GlobalLegacyIssuer";
        public const string ApiSettingGlobalTotpExpire = "GlobalTotpExpire";

        #endregion

        #region Policy Constants

        public const string PolicyForAdmins = "AdminsPolicy";
        public const string PolicyForServices = "ServicesPolicy";
        public const string PolicyForUsers = "UsersPolicy";

        #endregion

        #region Test Constants

        public const string ApiTestAudience = "AudienceUnitTests";
        public const string ApiTestAudiencePassCurrent = "te$tAudiencePa$$word01!";
        public const string ApiTestAudiencePassNew = "te$tAudiencePa$$word01!new";
        public const string ApiTestClaim = "ClaimUnitTests";
        public const string ApiTestClaimSubject = "ClaimUnitTestsSubject";
        public const string ApiTestClaimValueType = "ClaimUnitTestsValueType";
        public const string ApiTestEmailContent = "EmailUnitTestsContent";
        public const string ApiTestEmailSubject = "EmailUnitTestsSubject";
        public const string ApiTestIssuer = "IssuerUnitTests";
        public const string ApiTestIssuerKey = "IssuerUnitTestsKey";
        public const string ApiTestLogin = "LoginUnitTests";
        public const string ApiTestLoginKey = "LoginUnitTestsKey";
        public const string ApiTestMOTD = "MOTDUnitTests";
        public const string ApiTestRole = "RoleUnitTests";
        public const string ApiTestTextContent = "TextUnitTestsContent";
        public const string ApiTestUser = "unittestuser1@local";
        public const string ApiTestUserPhone = "+11111111111";
        public const string ApiTestUserPassCurrent = "te$tUserPa$$word01!";
        public const string ApiTestUserPassNew = "te$tUserPa$$word01!new";
        public const string ApiTestUri = "UrlUnitTests";
        public const string ApiTestUriLink = "https://app.test.net/1/callback";

        #endregion

        #region Messages

        public const string MsgConfirmEmailSubject = "Confirm Email Address";
        public const string MsgConfirmPasswordSubject = "Confirm Password";
        public const string MsgConfirmPhoneSubject = "Confirm Phone Number";
        public const string MsgConfirmNewUserSubject = "Confirm New User";

        #endregion
    }
}
