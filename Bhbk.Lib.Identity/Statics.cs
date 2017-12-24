using System;

namespace Bhbk.Lib.Identity
{
    //https://tools.ietf.org/html/rfc6749#section-2.1
    public enum AudienceType
    {
        user_agent,
        native,
        server
    }

    public enum GrantType
    {
        client_credentials,
        authorization_code,
        refresh_token,
        access_token
    }

    public class Statics
    {
        #region Attribute Settings
        public const String AttrAudienceIDV1 = "audience_id";
        public const String AttrAudienceIDV2 = "audiences";
        public const String AttrClientIDV1 = "client_id";
        public const String AttrClientIDV2 = "client";
        public const String AttrGrantTypeIDV1 = "grant_type";
        public const String AttrGrantTypeIDV2 = "grant_type";
        public const String AttrPasswordIDV1 = "password";
        public const String AttrPasswordIDV2 = "password";
        public const String AttrUserIDV1 = "username";
        public const String AttrUserIDV2 = "username";
        #endregion

        #region Configuration Settings
        public const String ApiDefaultClient = "Bhbk";
        public const String ApiDefaultAudienceUi = "Bhbk.WebUi.Identity";
        public const String ApiDefaultAudienceApi = "Bhbk.WebApi.Identity";
        public const String ApiDefaultPhone = "12223334444";
        public const String ApiDefaultLogin = "local";
        public const String ApiDefaultLoginKey = "sxW8wlsw1Z04heUckw";
        public const String ApiDefaultLoginName = "built-in";
        public const String ApiDefaultRoleForAdminUi = "(Built-In) Administrators";
        public const String ApiDefaultRoleForViewerApi = "(Built-In) Viewers";
        public const String ApiDefaultUserAdmin = "root@local";
        public const String ApiTokenConfirmEmail = "EmailConfirmation";
        public const String ApiTokenConfirmPhone = "PhoneNumberConfirmation";
        public const String ApiTokenConfirmTwoFactor = "TwoFactorConfirmation";
        public const String ApiTokenResetPassword = "ResetPassword";
        #endregion

        #region Configuration Settings (Unit Tests)
        public const String ApiUnitTestAudience = "Audience-UnitTests-";
        public const String ApiUnitTestClient = "Client-UnitTests-";
        public const String ApiUnitTestPasswordCurrent = "3uetw7W$mswU";
        public const String ApiUnitTestPasswordNew = "mv7wd3dks&k3";
        public const String ApiUnitTestRole = "Role-UnitTests-";
        public const String ApiUnitTestUser = "unit-tests@";
        public const String ApiUnitTestLogin = "Login-UnitTests-";
        public const String ApiUnitTestLoginKey = "Login-UnitTests-sxW8wlsw1Z04heUckw";
        public const String ApiUnitTestLoginName = "Login-UnitTests-";
        public const String ApiUnitTestClaimType = "ClaimType-UnitTests-";
        public const String ApiUnitTestClaimValue = "ClaimValue-UnitTests-";
        #endregion

        #region Message Settings
        public const String MsgAudienceAlreadyExists = "Audience already exists";
        public const String MsgAudienceImmutable = "Audience is immutable";
        public const String MsgAudienceInvalid = "Audience is invalid or disabled";
        public const String MsgClientAlreadyExists = "Client already exists";
        public const String MsgClientImmutable = "Client is immutable";
        public const String MsgClientInvalid = "Client is invalid or disabled";
        public const String MsgLoginAlreadyExists = "Login already exists";
        public const String MsgLoginImmutable = "Login is immutable";
        public const String MsgLoginInvalid = "Login invalid or disabled";
        public const String MsgRoleAlreadyExists = "Role already exists";
        public const String MsgRoleImmutable = "Role is immutable";
        public const String MsgRoleInvalid = "Role invalid or disabled";
        public const String MsgRoleNotExist = "Role does not exist";
        public const String MsgSystemParametersInvalid = "Parameters invalid";
        public const String MsgSystemExceptionCaught = "Exception was caught";
        public const String MsgUserAlreadyExists = "User already exists";
        public const String MsgUserImmutable = "User is immutable";
        public const String MsgUserInvalid = "User is invalid, locked or disabled.";
        public const String MsgUserInvalidCurrentEmail = "User current email incorrect";
        public const String MsgUserInvalidCurrentPassword = "User current password incorrect";
        public const String MsgUserInvalidCurrentPhone = "User current phone incorrect";
        public const String MsgUserInvalidEmailConfirm = "User new email does not match confirm email";
        public const String MsgUserInvalidPassword = "User password invalid";
        public const String MsgUserInvalidPasswordConfirm = "User new password does not match confirm password";
        public const String MsgUserInvalidPhoneConfirm = "User new phone does not match confirm phone";
        public const String MsgUserInvalidToken = "User token is invalid";
        public const String MsgUserInvalidTwoFactor = "User two factor is invalid";
        #endregion
    }
}
