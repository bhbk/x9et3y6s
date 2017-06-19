using System;

namespace Bhbk.Lib.Identity
{
    public enum AudienceType
    {
        ThinClient,
        ThickClient
    }

    public class Statics
    {
        #region Attribute Settings
        public const String AttrClientID = "client_id";
        public const String AttrAudienceID = "audience_id";
        public const String AttrUserID = "user_id";
        #endregion

        #region Configuration Settings
        public const String ApiDefaultAdmin = "root@local";
        public const String ApiDefaultConfiguration = "Global.config";
        public const String ApiDefaultAudience = "Bhbk.WebUi.Identity";
        public const String ApiDefaultClient = "Bhbk";
        public const String ApiDefaultProvider = "local";
        public const String ApiDefaultPhone = "12223334444";
        public const String ApiDefaultRoleForAdmin = "(Built-In) Administrators";
        public const String ApiDefaultRoleForViewer = "(Built-In) Viewers";
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
        public const String ApiUnitTestProvider = "Provider-UnitTests-";
        public const String ApiUnitTestRole = "Role-UnitTests-";
        public const String ApiUnitTestUserDisplayName = "User-UnitTests-";
        public const String ApiUnitTestUserEmail = "unit-tests@";
        public const String ApiUnitTestClaimType = "ClaimType-UnitTests-";
        public const String ApiUnitTestClaimValue = "ClaimValue-UnitTests-";
        #endregion

        #region Message Settings
        public const String MsgAudienceAlreadyExists = "Audience already exists";
        public const String MsgAudienceImmutable = "Audience is immutable";
        public const String MsgAudienceInvalid = "Audience is invalid";
        public const String MsgAudienceNotExist = "Audience does not exist";
        public const String MsgClientAlreadyExists = "Client already exists";
        public const String MsgClientImmutable = "Client is immutable";
        public const String MsgClientInvalid = "Client is invalid";
        public const String MsgClientNotExist = "Client does not exist";
        public const String MsgProviderAlreadyExists = "Provider already exists";
        public const String MsgProviderImmutable = "Provider is immutable";
        public const String MsgProviderInvalid = "Provider invalid";
        public const String MsgProviderNotExist = "Provider does not exist";
        public const String MsgRoleAlreadyExists = "Role already exists";
        public const String MsgRoleImmutable = "Role is immutable";
        public const String MsgRoleInvalid = "Role invalid";
        public const String MsgRoleNotAdded = "Role was not added";
        public const String MsgRoleNotExist = "Role does not exist";
        public const String MsgRoleNotRemoved = "Role was not removed";
        public const String MsgUserAlreadyExists = "User already exists";
        public const String MsgUserImmutable = "User is immutable";
        public const String MsgUserInvalid = "User is invalid, locked or disabled";
        public const String MsgUserInvalidCurrentEmail = "User current email incorrect";
        public const String MsgUserInvalidCurrentPassword = "User current password incorrect";
        public const String MsgUserInvalidCurrentPhone = "User current phone incorrect";
        public const String MsgUserInvalidEmailConfirm = "User new email does not match confirm email";
        public const String MsgUserInvalidPasswordConfirm = "User new password does not match confirm password";
        public const String MsgUserInvalidPhoneConfirm = "User new phone does not match confirm phone";
        public const String MsgUserLocked = "User is locked";
        public const String MsgUserNotExist = "User does not exist";
        public const String MsgUserPasswordExists = "User already has password";
        public const String MsgUserPasswordNotExists = "User does not have password";
        public const String MsgUserInvalidToken = "User token is invalid";
        public const String MsgUserTwoFactorAlreadyExists = "User two factor invalid";
        public const String MsgUserUnconfirmed = "User is unconfirmed";
        #endregion
    }
}
