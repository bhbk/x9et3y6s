using Bhbk.Lib.Identity.Models;
using System;

namespace Bhbk.Lib.Identity
{
    //https://tools.ietf.org/html/rfc6749#section-2.1
    public enum AudienceType
    {
        user_agent,
        native,
        server,
    }

    public enum GrantType
    {
        access_token,
        authorize_code,
        client_credential,
        refresh_token,
    }

    public enum TaskType
    {
        MaintainActivity,
        MaintainQuotes,
        MaintainTokens,
        MaintainUsers,
    }

    public class Statics
    {
        #region Attribute Constants

        public const string AttrAudienceIDV1 = "audience_id";
        public const string AttrAudienceIDV2 = "audience";
        public const string AttrAuthorizeCodeIDV1 = "code";
        public const string AttrAuthorizeCodeIDV2 = "code";
        public const string AttrClientIDV1 = "client_id";
        public const string AttrClientIDV2 = "client";
        public const string AttrClientSecretIDV1 = "client_secret";
        public const string AttrClientSecretIDV2 = "client_secret";
        public const string AttrGrantTypeIDV1 = "grant_type";
        public const string AttrGrantTypeIDV2 = "grant_type";
        public const string AttrRedirectUriIDV1 = "redirect_uri";
        public const string AttrRedirectUriIDV2 = "redirect_uri";
        public const string AttrRefreshTokenIDV1 = "refresh_token";
        public const string AttrRefreshTokenIDV2 = "refresh_token";
        public const string AttrUserIDV1 = "username";
        public const string AttrUserIDV2 = "user";
        public const string AttrUserPasswordIDV1 = "password";
        public const string AttrUserPasswordIDV2 = "password";

        #endregion

        #region Configuration Constants

        public const string ApiDefaultClient = "Bhbk";
        public const string ApiDefaultAudienceUi = "Bhbk.WebUi.Identity";
        public const string ApiDefaultAudienceApi = "Bhbk.WebApi.Identity";
        public const string ApiDefaultPhone = "+00000000000";
        public const string ApiDefaultLogin = "local";
        public const string ApiDefaultLoginKey = "sxW8wlsw1Z04heUckw";
        public const string ApiDefaultLoginName = "local";
        public const string ApiDefaultRoleForAdminUi = "(Built-In) Administrators";
        public const string ApiDefaultRoleForViewerApi = "(Built-In) Viewers";
        public const string ApiDefaultUserAdmin = "root@local";
        public const string ApiDefaultUserPassword = "pa$$word01!";
        public const string ApiDefaultFirstName = "System";
        public const string ApiDefaultLastName = "Administrator";

        #endregion

        #region Configuration Constants (Unit Tests)

        public const string ApiUnitTestAudienceA = "AudienceUnitTestsA";
        public const string ApiUnitTestAudienceB = "AudienceUnitTestsB";
        public const string ApiUnitTestClientA = "ClientUnitTestsA";
        public const string ApiUnitTestClientB = "ClientUnitTestsB";
        public const string ApiUnitTestClaimType = "ClaimTypeUnitTests";
        public const string ApiUnitTestClaimValue = "ClaimValueUnitTests";
        public const string ApiUnitTestLoginA = "LoginUnitTestsA";
        public const string ApiUnitTestLoginAKey = "LoginUnitTestsAKey";
        public const string ApiUnitTestLoginB = "LoginUnitTestsB";
        public const string ApiUnitTestLoginBKey = "LoginUnitTestsBKey";
        public const string ApiUnitTestRoleA = "RoleUnitTestsA";
        public const string ApiUnitTestRoleB = "RoleUnitTestsB";
        public const string ApiUnitTestUserA = "UserUnitTestsA@local";
        public const string ApiUnitTestUserB = "UserUnitTestsB@local";
        public const string ApiUnitTestUserPassCurrent = "pa$$word01!";
        public const string ApiUnitTestUserPassNew = "pa$$word01!new";
        public const string ApiUnitTestUriA = "UrlUnitTestsA";
        public const string ApiUnitTestUriALink = "https://app.test.net/a/redirect";
        public const string ApiUnitTestUriB = "UrlUnitTestsB";
        public const string ApiUnitTestUriBLink = "https://app.test.net/b/redirect";

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
        public const string MsgSysQueueEmailError = "Fail to queue email message for delivery";
        public const string MsgSysQueueSmsError = "Fail to queue SMS message for delivery";
        public const string MsgUserAlreadyExists = "User already exists";
        public const string MsgUserImmutable = "User is immutable";
        public const string MsgUserInvalid = "User is invalid, locked or disabled.";
        public const string MsgUriAlreadyExists = "Uri already exists";
        public const string MsgUriImmutable = "Uri is immutable";
        public const string MsgUriInvalid = "Uri is invalid or disabled.";
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

        #region Messages (Email)

        public const string ApiEmailConfirmEmailSubject = "Confirm Email Address";
        public const string ApiEmailConfirmPasswordSubject = "Confirm Password";
        public const string ApiEmailConfirmPhoneSubject = "Confirm Phone Number";
        public const string ApiEmailConfirmNewUserSubject = "Confirm New User";

        //https://htmlformatter.com/, https://www.freeformatter.com/java-dotnet-escape.html

        public static string ApiEmailConfirmEmailHtml(AppUser user, Uri link)
        {
            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w=\r\n3.org/TR/REC-html40/loose.dtd\">\r\n" +
                "<html xmlns=3D \"http://www.w3.org/1999/xhtml\">\r\n" +
                "<head>\r\n" +
                "    <!--[if !mso]><!-- -->\r\n" +
                "    <meta http-equiv=3D \"Content-Type\" content=3D \"text/html; charset=3Dutf-8\">\r\n" +
                "    <style>\r\n" +
                "        @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?f= amily=3DOpen+Sans'); }\r\n" +
                "    </style>\r\n" +
                "    <!--<![endif]-->\r\n" +
                "    <style>\r\n" +
                "        table { color: inherit; }\r\n" +
                "    </style>\r\n" +
                "</head>\r\n" +
                "<body style=3D \"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:=#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=3D \"0\" marginwidth=3D \"0\" id=3D \"dbx-email-body\">\r\n" +
                "    <div style=3D \"max-width: 600px !important; padding: 4px;\">\r\n" +
                "        <table cellpadding=3D \"0\" cellspacing==3D \"0\" style=3D \"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=3D \"0\" align==3D \"center\">\r\n" +
                "            <tr>\r\n" +
                "                <td align=3D \"center\">\r\n" +
                "                    <table cellpadding=3D \"0\" cellspacing=3D \"0\" border=3D \"0\" width=3D \"100%\">\r\n" +
                "                        <tr style=3D \"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">\r\n" +
                "                            <td>\r\n" +
                "                                <br>\r\n" +
                "                                <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + ",\r\n" +
                "                                <br>\r\n" +
                "                                <br>We just need to verify your email address b= efore your sign up is complete!\r\n" +
                "                                <br>\r\n" +
                "                                <br><a style=3D \"border-radius: 4px; font=\r\n-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: \"Open Sans\", \"Helvetica Neue\", Arial; margin: 0; display: block; background-color:#007ee6; text-align: center;\" href=3D \"" + link.AbsoluteUri + "\">Verify your email</a>\r\n" +
                "                                <br>Happy Dropboxing!" +
                "                           </td>\r\n" +
                "                        <tr>\r\n" +
                "                        <tr>\r\n" +
                "                           <td height=3D \"40\"></td>\r\n" +
                "                        </tr>\r\n" +
                "                    </table>\r\n" +
                "                </td>\r\n" +
                "            </tr>\r\n" +
                "        </table>\r\n" +
                "    </div>\r\n" +
                "</body>\r\n" +
                "</html>";
        }

        public static string ApiEmailConfirmPasswordHtml(AppUser user, Uri link)
        {
            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w3.org/TR/REC-html40/loose.dtd\">\r\n" +
                "<html xmlns=3D \"http://www.w3.org/1999/xhtml\">\r\n" +
                "<head>\r\n" +
                "    <!--[if !mso]><!-- -->\r\n" +
                "    <style>\r\n" +
                "        @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?family=3DOpen+Sans'); }\r\n" +
                "    </style>\r\n" +
                "    <!--<![endif]-->\r\n" +
                "    <style>\r\n" +
                "        table { color: inherit; }\r\n" +
                "    </style>\r\n" +
                "</head>\r\n" +
                "<body style=3D \"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=3D \"0\" marginwidth=3D \"0\" id=3D \"dbx-email-body\">\r\n" +
                "    <div style=3D \"max-width: 600px !important; padding: 4px;\">\r\n" +
                "        <table cellpadding=3D \"0\" cellspacing=3D \"0\" style=3D \"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=3D \"0\" align==3D \"center\">\r\n" +
                "            <tr>\r\n" +
                "                <td align=3D \"center\">\r\n" +
                "                    <table cellpadding=3D \"0\" cellspacing=3D \"0\" border=3D \"0\" width=3D \"100%\">\r\n" +
                "                        <tr style=3D \"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">\r\n" +
                "                            <td>\r\n" +
                "                                <br>\r\n" +
                "                                <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + ",\r\n" +
                "                                <br>\r\n" +
                "                                <br>Someone recently requested a password change= for your account. If this was you, you can set a new password here= :\r\n" +
                "                                <br>\r\n" +
                "                                <br><a style=3D 'border-radius: 4px; font-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: \"Open Sans\", \"Helvetica Neue\", Arial; margin: 0; display: block; background-color: #007ee6; text-align: center;' href=3D \"" + link.AbsoluteUri + "\">Reset password</a>\r\n" +
                "                                <br>If you don't want to change your password or didn't request this, just ignore and delete this message.\r\n" +
                "                                <br>\r\n" +
                "                                <br>To keep your account secure, please don't forward this email to anyone.\r\n" +
                "                                <br>\r\n" +
                "                            </td>\r\n" +
                "                        </tr>\r\n" +
                "                        <tr>\r\n" +
                "                            <td height=3D \"40\"></td>\r\n" +
                "                        </tr>\r\n" +
                "                    </table>\r\n" +
                "                </td>\r\n" +
                "            </tr>\r\n" +
                "        </table>\r\n" +
                "    </div>\r\n" +
                "</body>\r\n" +
                "</html>";
        }

        public static string ApiEmailConfirmPhoneHtml(AppUser user, Uri link)
        {
            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w3.org/TR/REC-html40/loose.dtd\">\r\n" +
                "<html xmlns=3D \"http://www.w3.org/1999/xhtml\">\r\n" +
                "<head>\r\n" +
                "    <!--[if !mso]><!-- -->\r\n" +
                "    <style>\r\n" +
                "        @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?family=3DOpen+Sans'); }\r\n" +
                "    </style>\r\n" +
                "    <!--<![endif]-->\r\n" +
                "    <style>\r\n" +
                "        table { color: inherit; }\r\n" +
                "    </style>\r\n" +
                "</head>\r\n" +
                "<body style=3D \"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=3D \"0\" marginwidth=3D \"0\" id=3D \"dbx-email-body\">\r\n" +
                "    <div style=3D \"max-width: 600px !important; padding: 4px;\">\r\n" +
                "        <table cellpadding=3D \"0\" cellspacing=3D \"0\" style=3D \"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=3D \"0\" align==3D \"center\">\r\n" +
                "            <tr>\r\n" +
                "                <td align=3D \"center\">\r\n" +
                "                    <table cellpadding=3D \"0\" cellspacing=3D \"0\" border=3D \"0\" width=3D \"100%\">\r\n" +
                "                        <tr style=3D \"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">\r\n" +
                "                            <td>\r\n" +
                "                                <br>\r\n" +
                "                                <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + ",\r\n" +
                "                                <br>\r\n" +
                "                                <br>Someone recently requested a password change= for your account. If this was you, you can set a new password here= :\r\n" +
                "                                <br>\r\n" +
                "                                <br><a style=3D 'border-radius: 4px; font-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: \"Open Sans\", \"Helvetica Neue\", Arial; margin: 0; display: block; background-color: #007ee6; text-align: center;' href=3D \"" + link.AbsoluteUri + "\">Reset password</a>\r\n" +
                "                                <br>If you don't want to change your password or didn't request this, just ignore and delete this message.\r\n" +
                "                                <br>\r\n" +
                "                                <br>To keep your account secure, please don't forward this email to anyone.\r\n" +
                "                                <br>\r\n" +
                "                            </td>\r\n" +
                "                        </tr>\r\n" +
                "                        <tr>\r\n" +
                "                            <td height=3D \"40\"></td>\r\n" +
                "                        </tr>\r\n" +
                "                    </table>\r\n" +
                "                </td>\r\n" +
                "            </tr>\r\n" +
                "        </table>\r\n" +
                "    </div>\r\n" +
                "</body>\r\n" +
                "</html>";
        }

        public static string ApiEmailConfirmNewUserHtml(AppClient client, AppUser user, Uri link)
        {
            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w=\r\n3.org/TR/REC-html40/loose.dtd\">\r\n" +
                "<html xmlns=3D \"http://www.w3.org/1999/xhtml\">\r\n" +
                "<head>\r\n" +
                "    <!--[if !mso]><!-- -->\r\n" +
                "    <meta http-equiv=3D \"Content-Type\" content=3D \"text/html; charset=3Dutf-8\">\r\n" +
                "    <style>\r\n" +
                "        @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?f= amily=3DOpen+Sans'); }\r\n" +
                "    </style>\r\n" +
                "    <!--<![endif]-->\r\n" +
                "    <style>\r\n" +
                "        table { color: inherit; }\r\n" +
                "    </style>\r\n" +
                "</head>\r\n" +
                "<body style=3D \"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:=#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=3D \"0\" marginwidth=3D \"0\" id=3D \"dbx-email-body\">\r\n" +
                "    <div style=3D \"max-width: 600px !important; padding: 4px;\">\r\n" +
                "        <table cellpadding=3D \"0\" cellspacing==3D \"0\" style=3D \"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=3D \"0\" align==3D \"center\">\r\n" +
                "            <tr>\r\n" +
                "                <td align=3D \"center\">\r\n" +
                "                    <table cellpadding=3D \"0\" cellspacing=3D \"0\" border=3D \"0\" width=3D \"100%\">\r\n" +
                "                        <tr style=3D \"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">\r\n" +
                "                            <td>\r\n" +
                "                                <br>\r\n" +
                "                                <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + ",\r\n" +
                "                                <br>\r\n" +
                "                                <br>We just need to verify your email address b= efore your sign up is complete!\r\n" +
                "                                <br>\r\n" +
                "                                <br><a style=3D \"border-radius: 4px; font=\r\n-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: \"Open Sans\", \"Helvetica Neue\", Arial; margin: 0; display: block; background-color:#007ee6; text-align: center;\" href=3D \"" + link.AbsoluteUri + "\">Verify your email</a>\r\n" +
                "                                <br>Happy Dropboxing!" +
                "                           </td>\r\n" +
                "                        <tr>\r\n" +
                "                        <tr>\r\n" +
                "                           <td height=3D \"40\"></td>\r\n" +
                "                        </tr>\r\n" +
                "                    </table>\r\n" +
                "                </td>\r\n" +
                "            </tr>\r\n" +
                "        </table>\r\n" +
                "    </div>\r\n" +
                "</body>\r\n" +
                "</html>";
        }

        #endregion

        #region Parameter Constants

        public const string GetOrderBy = "orderBy";
        public const string GetPageSize = "pageSize";
        public const string GetPageNumber = "pageNumber";

        #endregion
    }
}