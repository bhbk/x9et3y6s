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

        public const string DefaultAudience_Alert = "Alert";
        public const string DefaultAudiencePassword_Alert = "4Boj4NIFCEZd0D2tyinPVlJ9yzHBl3fV";
        public const string DefaultAudience_Identity = "Identity";
        public const string DefaultAudiencePassword_Identity = "eBr3r3N1L6JV9jewYJOS6fjZ7EJGeGcb";
        public const string DefaultIssuer = "Local";
        public const string DefaultIssuerKey = "x6ZFCKCAtuJ0uuunjbi26O6A2dHFFsf877qpdcqOXqZvjABAHkBjkbb29YNjD75B";
        public const string DefaultLogin = "Local";
        public const string DefaultLoginKey = "yUrOgQ1RZefUZ5DnC153sLDy23hmSOwn";
        public const string DefaultLoginName = "Local";
        public const string DefaultPolicyForHumans = "HumansPolicy";
        public const string DefaultPolicyForServices = "ServicesPolicy";
        public const string DefaultRoleForAdmin_Alert = "Alert.Admins";
        public const string DefaultRoleForUser_Alert = "Alert.Users";
        public const string DefaultRoleForAdmin_Identity = "Identity.Admins";
        public const string DefaultRoleForUser_Identity = "Identity.Users";
        public const string DefaultUser_Admin = "admin@local";
        public const string DefaultUserPass_Admin = "pa$$word01!";
        public const string DefaultUserFirstName_Admin = "Administrator";
        public const string DefaultUserLastName_Admin = "User";
        public const string DefaultUser_Normal = "user@local";
        public const string DefaultUserPass_Normal = "pa$$word02!";
        public const string DefaultUserFirstName_Normal = "Normal";
        public const string DefaultUserLastName_Normal = "User";
        public const string SettingAccessExpire = "AccessExpire";
        public const string SettingRefreshExpire = "RefreshExpire";
        public const string SettingTotpExpire = "TotpExpire";
        public const string SettingPollingMax = "PollingMax";
        public const string SettingGlobalLegacyClaims = "GlobalLegacyClaims";
        public const string SettingGlobalLegacyIssuer = "GlobalLegacyIssuer";
        public const string SettingGlobalTotpExpire = "GlobalTotpExpire";
        public const string SettingTheySaidSoLicense = "TheySaidSoLicense";
        public const string SettingTheySaidSoUrl = "TheySaidSoUrl";

        #endregion

        #region Test Constants

        public const string TestAudience = "AudienceTests";
        public const string TestAudiencePassCurrent = "te$tAudiencePa$$word01!";
        public const string TestAudiencePassNew = "te$tAudiencePa$$word01!new";
        public const string TestClaim = "ClaimTests";
        public const string TestClaimSubject = "ClaimTestsSubject";
        public const string TestClaimValueType = "ClaimTestsValueType";
        public const string TestEmailContent = "EmailTestsContent";
        public const string TestEmailSubject = "EmailTestsSubject";
        public const string TestIssuer = "IssuerTests";
        public const string TestIssuerKey = "IssuerTestsKey";
        public const string TestLogin = "LoginTests";
        public const string TestLoginKey = "LoginTestsKey";
        public const string TestMotdAuthor = "MOTDTestsAuthor";
        public const string TestRole = "RoleTests";
        public const string TestTextContent = "TextTestsContent";
        public const string TestUser = "testuser1@localhost.localdomain";
        public const string TestUserPhoneNumber = "+12223334444";
        public const string TestUserPassCurrent = "te$tUserPa$$word01!";
        public const string TestUserPassNew = "te$tUserPa$$word01!new";
        public const string TestUri = "UrlTests";
        public const string TestUriLink = "https://app.test.net/1/callback";

        #endregion

        #region Messages

        public const string MsgConfirmEmailSubject = "Confirm Email Address";
        public const string MsgConfirmPasswordSubject = "Confirm Password";
        public const string MsgConfirmPhoneSubject = "Confirm Phone Number";
        public const string MsgConfirmNewUserSubject = "Confirm New User";

        #endregion
    }
}
