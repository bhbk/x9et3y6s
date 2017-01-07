using System;
using System.Configuration;
using System.Text.RegularExpressions;

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
        public static readonly Int16 ApiDefaultPassMinLength = Int16.Parse(ConfigurationManager.AppSettings["DefaultPassMinLength"]);
        public static readonly Int16 ApiDefaultTokenExpire = Int16.Parse(ConfigurationManager.AppSettings["DefaultTokenExpire"]);
        public const String ApiDefaultRealm = "local";
        public const String ApiDefaultRoleForAdmin = "(Built-In) Administrators";
        public const String ApiDefaultRoleForViewer = "(Built-In) Viewers";
        public const String ApiDefaultClient = "Bhbk";
        public const String ApiDefaultAudience = "Bhbk.WebUi.Identity";
        public const String ApiUnitTestsPassword = "3uetw7W$mswU";
        public const String ApiUnitTestsPasswordNew = "mv7wd3dks&k3";
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
        public const String MsgRoleAlreadyExists = "Role already exists";
        public const String MsgRoleImmutable = "Role is immutable";
        public const String MsgRoleInvalid = "Role invalid";
        public const String MsgRoleNotAdded = "Role was not added";
        public const String MsgRoleNotExist = "Role does not exist";
        public const String MsgRoleNotRemoved = "Role was not removed";
        public const String MsgRealmAlreadyExists = "Realm already exists";
        public const String MsgRealmImmutable = "Realm is immutable";
        public const String MsgRealmInvalid = "Realm invalid";
        public const String MsgRealmNotExist = "Realm does not exist";
        public const String MsgTokenInvalid = "Token is invalid";
        public const String MsgUserAlreadyExists = "User already exists";
        public const String MsgUserImmutable = "User is immutable";
        public const String MsgUserInvalid = "User is invalid, locked or disabled";
        public const String MsgUserInvalidFormat = "User name has invalid format";
        public const String MsgUserInvalidCurrentPassword = "User current password incorrect";
        public const String MsgUserInvalidNewPasswordConfirm = "User new password does not match confirm new password";
        public const String MsgUserNoAccess = "User has no access";
        public const String MsgUserNotExist = "User does not exist";
        #endregion
    }
}
