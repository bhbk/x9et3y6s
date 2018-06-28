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
        #region Attribute Constants
        public const string AttrAudienceIDV1 = "audience_id";
        public const string AttrAudienceIDV2 = "audience";
        public const string AttrClientIDV1 = "client_id";
        public const string AttrClientIDV2 = "client";
        public const string AttrGrantTypeIDV1 = "grant_type";
        public const string AttrGrantTypeIDV2 = "grant_type";
        public const string AttrPasswordIDV1 = "password";
        public const string AttrPasswordIDV2 = "password";
        public const string AttrRefreshTokenIDV1 = "refresh_token";
        public const string AttrRefreshTokenIDV2 = "refresh_token";
        public const string AttrUserIDV1 = "username";
        public const string AttrUserIDV2 = "user";
        #endregion

        #region Configuration Constants
        public const string ApiDefaultClient = "Bhbk";
        public const string ApiDefaultAudienceUi = "Bhbk.WebUi.Identity";
        public const string ApiDefaultAudienceApi = "Bhbk.WebApi.Identity";
        public const string ApiDefaultPhone = "0000000000";
        public const string ApiDefaultLogin = "local";
        public const string ApiDefaultLoginKey = "sxW8wlsw1Z04heUckw";
        public const string ApiDefaultLoginName = "local";
        public const string ApiDefaultRoleForAdminUi = "(Built-In) Administrators";
        public const string ApiDefaultRoleForViewerApi = "(Built-In) Viewers";
        public const string ApiDefaultUserAdmin = "root@local";
        public const string ApiDefaultUserPassword = "3uetw7W$mswU";
        public const string ApiDefaultFirstName = "First";
        public const string ApiDefaultLastName = "Last";
        public const string ApiTokenConfirmEmail = "EmailConfirmation";
        public const string ApiTokenConfirmPhone = "PhoneNumberConfirmation";
        public const string ApiTokenConfirmTwoFactor = "TwoFactorConfirmation";
        public const string ApiTokenResetPassword = "ResetPassword";
        #endregion

        #region Configuration Constants (Unit Tests)
        public const string ApiUnitTestAudienceA = "AudienceUnitTestsA";
        public const string ApiUnitTestAudienceB = "AudienceUnitTestsB";
        public const string ApiUnitTestClientA = "ClientUnitTestsA";
        public const string ApiUnitTestClientB = "ClientUnitTestsB";
        public const string ApiUnitTestPasswordCurrent = "3uetw7W$mswU";
        public const string ApiUnitTestPasswordNew = "mv7wd3dks&k3";
        public const string ApiUnitTestRoleA = "RoleUnitTestsA";
        public const string ApiUnitTestRoleB = "RoleUnitTestsB";
        public const string ApiUnitTestUserA = "UserUnitTestsA@local";
        public const string ApiUnitTestUserB = "UserUnitTestsB@local";
        public const string ApiUnitTestLoginA = "LoginUnitTestsA";
        public const string ApiUnitTestLoginB = "LoginUnitTestsB";
        public const string ApiUnitTestLoginKeyA = "LoginUnitTestsKeyA";
        public const string ApiUnitTestLoginNameA = "LoginUnitTests";
        public const string ApiUnitTestClaimType = "ClaimTypeUnitTests";
        public const string ApiUnitTestClaimValue = "ClaimValueUnitTests";
        #endregion

        #region Messages
        public const string MsgAudienceAlreadyExists = "Audience already exists";
        public const string MsgAudienceImmutable = "Audience is immutable";
        public const string MsgAudienceInvalid = "Audience is invalid or disabled";
        public const string MsgClientAlreadyExists = "Client already exists";
        public const string MsgClientImmutable = "Client is immutable";
        public const string MsgClientInvalid = "Client is invalid or disabled";
        public const string MsgLoginAlreadyExists = "Login already exists";
        public const string MsgLoginImmutable = "Login is immutable";
        public const string MsgLoginInvalid = "Login invalid or disabled";
        public const string MsgRoleAlreadyExists = "Role already exists";
        public const string MsgRoleImmutable = "Role is immutable";
        public const string MsgRoleInvalid = "Role invalid or disabled";
        public const string MsgRoleNotExist = "Role does not exist";
        public const string MsgSysNotImplemented = "Feature not implemented yet";
        public const string MsgSysParamsInvalid = "One or more parameters invalid";
        public const string MsgUserAlreadyExists = "User already exists";
        public const string MsgUserImmutable = "User is immutable";
        public const string MsgUserInvalid = "User is invalid, locked or disabled.";
        public const string MsgUserInvalidCurrentEmail = "User current email incorrect";
        public const string MsgUserInvalidCurrentPassword = "User current password incorrect";
        public const string MsgUserInvalidCurrentPhone = "User current phone incorrect";
        public const string MsgUserInvalidEmailConfirm = "User new email does not match confirm email";
        public const string MsgUserInvalidPassword = "User password invalid";
        public const string MsgUserInvalidPasswordConfirm = "User new password does not match confirm password";
        public const string MsgUserInvalidPhoneConfirm = "User new phone does not match confirm phone";
        public const string MsgUserInvalidToken = "User token is invalid";
        public const string MsgUserInvalidTwoFactor = "User two factor is invalid";
        #endregion

        #region Parameter Constants
        public const string GetOrderBy = "orderBy";
        public const string GetPageSize = "pageSize";
        public const string GetPageNumber = "pageNumber";
        #endregion
    }
}
