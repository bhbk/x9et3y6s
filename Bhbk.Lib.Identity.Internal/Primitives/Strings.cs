using Bhbk.Lib.Identity.Internal.EntityModels;
using System;

namespace Bhbk.Lib.Identity.Internal.Primitives
{
    public class Strings
    {
        #region Attribute Constants

        public const string AttrAuthorizeCodeIDV1 = "code";
        public const string AttrAuthorizeCodeIDV2 = "code";
        public const string AttrIssuerIDV1 = "issuer_id";
        public const string AttrIssuerIDV2 = "issuer";
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

        public const string ApiDefaultClientUi = "Bhbk.WebUi.Identity";
        public const string ApiDefaultClientApi = "Bhbk.WebApi.Identity";
        public const string ApiDefaultFormatDate = "yyyy/M/dd h:mm:s tt";
        public const string ApiDefaultIssuer = "Bhbk";
        public const string ApiDefaultPhone = "+00000000000";
        public const string ApiDefaultLogin = "local";
        public const string ApiDefaultLoginKey = "local";
        public const string ApiDefaultLoginName = "local";
        public const string ApiDefaultRoleForAdmin = "Bhbk.WebApi.Identity(Admins)";
        public const string ApiDefaultRoleForUser = "Bhbk.WebApi.Identity(Users)";
        public const string ApiDefaultUserAdmin = "root@local";
        public const string ApiDefaultUserPassword = "pa$$word01!";
        public const string ApiDefaultFirstName = "System";
        public const string ApiDefaultLastName = "Administrator";

        #endregion

        #region Configuration Constants (Unit Tests)

        public const string ApiUnitTestClient1 = "ClientUnitTests1";
        public const string ApiUnitTestClient1Key = "ClientUnitTests1Key";
        public const string ApiUnitTestClient2 = "ClientUnitTests2";
        public const string ApiUnitTestClient2Key = "ClientUnitTests2Key";
        public const string ApiUnitTestClaim1 = "ClaimUnitTests1";
        public const string ApiUnitTestClaim2 = "ClaimUnitTests2";
        public const string ApiUnitTestEmailContent = "EmailUnitTestsContent";
        public const string ApiUnitTestEmailSubject = "EmailUnitTestsSubject";
        public const string ApiUnitTestIssuer1 = "IssuerUnitTests1";
        public const string ApiUnitTestIssuer1Key = "IssuerUnitTests1Key";
        public const string ApiUnitTestIssuer2 = "IssuerUnitTests2";
        public const string ApiUnitTestIssuer2Key = "IssuerUnitTests2Key";
        public const string ApiUnitTestLogin1 = "LoginUnitTests1";
        public const string ApiUnitTestLogin1Key = "LoginUnitTests1Key";
        public const string ApiUnitTestLogin2 = "LoginUnitTests2";
        public const string ApiUnitTestLogin2Key = "LoginUnitTests2Key";
        public const string ApiUnitTestRole1 = "RoleUnitTests1";
        public const string ApiUnitTestRole2 = "RoleUnitTests2";
        public const string ApiUnitTestUser1 = "unittestuser1@local";
        public const string ApiUnitTestUser1Phone = "+00000000000";
        public const string ApiUnitTestUser2 = "unittestuser2@local";
        public const string ApiUnitTestUser2Phone = "+00000000000";
        public const string ApiUnitTestUserPassCurrent = "te$tpa$$word01!";
        public const string ApiUnitTestUserPassNew = "te$tpa$$word01!new";
        public const string ApiUnitTestUri1 = "UrlUnitTests1";
        public const string ApiUnitTestUri1Link = "https://app.test.net/1/redirect";
        public const string ApiUnitTestUri2 = "UrlUnitTests2";
        public const string ApiUnitTestUri2Link = "https://app.test.net/2/redirect";

        #endregion

        #region Messages

        public const string MsgClaimAlreadyExists = "Claim already exists";
        public const string MsgClaimImmutable = "Claim is immutable";
        public const string MsgClaimInvalid = "Claim is invalid or disabled";
        public const string MsgClaimNotExist = "Claim does not exist";
        public const string MsgClientAlreadyExists = "Client already exists";
        public const string MsgClientImmutable = "Client is immutable";
        public const string MsgClientInvalid = "Client is invalid or disabled";
        public const string MsgClientNotExist = "Client does not exist";
        public const string MsgIssuerAlreadyExists = "Issuer already exists";
        public const string MsgIssuerImmutable = "Issuer is immutable";
        public const string MsgIssuerInvalid = "Issuer is invalid or disabled";
        public const string MsgIssuerNotExist = "Issuer does not exist";
        public const string MsgLoginAlreadyExists = "Login already exists";
        public const string MsgLoginImmutable = "Login is immutable";
        public const string MsgLoginInvalid = "Login invalid or disabled";
        public const string MsgLoginNotExist = "Login does not exist";
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
        public const string MsgUserNotExist = "User does not exist";
        public const string MsgUriAlreadyExists = "Uri already exists";
        public const string MsgUriImmutable = "Uri is immutable";
        public const string MsgUriInvalid = "Uri is invalid or disabled.";
        public const string MsgUriNotExist = "Uri does not exist";
        public const string MsgUserInvalidCurrentEmail = "User current email incorrect";
        public const string MsgUserInvalidCurrentPassword = "User current password incorrect";
        public const string MsgUserInvalidCurrentPhone = "User current phone incorrect";
        public const string MsgUserInvalidEmailConfirm = "User new email does not match confirm email";
        public const string MsgUserInvalidPassword = "User password invalid";
        public const string MsgUserInvalidPasswordConfirm = "User new password does not match confirm password";
        public const string MsgUserInvalidPhoneConfirm = "User new phone does not match confirm phone";
        public const string MsgUserTokenInvalid = "User token is invalid";
        public const string MsgUserInvalidTwoFactor = "User two factor is invalid";

        #endregion

        #region Messages (Templates)

        public const string ApiMsgConfirmEmailSubject = "Confirm Email Address";
        public const string ApiMsgConfirmPasswordSubject = "Confirm Password";
        public const string ApiMsgConfirmPhoneSubject = "Confirm Phone Number";
        public const string ApiMsgConfirmNewUserSubject = "Confirm New User";

        //https://htmlformatter.com/, https://www.freeformatter.com/java-dotnet-escape.html

        public static string ApiTemplateConfirmEmail(AppUser user, Uri link)
        {
            //use http://rendera.herokuapp.com/ to test template before format...
            //use https://www.buildmystring.com to format template into string that compiles...

            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w3.org/TR/REC-html40/loose.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "  <head>" +
            "    <!--[if !mso]><!-- -->" +
            "    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
            "    <style>" +
            "      @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?family=Open+Sans'); }" +
            "    </style>" +
            "    <!--<![endif]-->" +
            "    <style>" +
            "      table { color: inherit; }" +
            "    </style>" +
            "  </head>" +
            "  <body style=\"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=\"0\" marginwidth=\"0\" id=\"dbx-email-body\">" +
            "    <div style=\"max-width: 600px !important; padding: 4px;\">" +
            "      <table cellpadding=\"0\" cellspacing=\"0\" style=\"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=\"0\" align==\"center\">" +
            "        <tr>" +
            "          <td align=\"center\">" +
            "            <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"100%\">" +
            "              <tr style=\"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">" +
            "                <td>" +
            "                  <br>" +
            "                  <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + "." +
            "                  <br>" +
            "                  <br>Someone recently requested an email change for your account. If this was you, you can set a new password below." +
            "                  <br>" +
            "                  <br><a style= 'border-radius: 4px; font-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: \"Open Sans\", \"Helvetica Neue\", Arial; margin: 0; display: block; background-color: #007ee6; text-align: center;' href=" + link.AbsoluteUri + ">Change email address</a>" +
            "                  <br>If you don't want to change your email address or didn't request this, just ignore and delete this message." +
            "                  <br>" +
            "                  <br>To keep your account secure, please don't forward this email to anyone." +
            "                  <br>" +
            "                </td>" +
            "              </tr>" +
            "              <tr>" +
            "                <td height=\"40\"></td>" +
            "              </tr>" +
            "            </table>" +
            "          </td>" +
            "        </tr>" +
            "      </table>" +
            "    </div>" +
            "  </body>" +
            "</html>";
        }

        public static string ApiTemplateConfirmPassword(AppUser user, Uri link)
        {
            //use http://rendera.herokuapp.com/ to test template before format...
            //use https://www.buildmystring.com to format template into string that compiles...

            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w3.org/TR/REC-html40/loose.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "  <head>" +
            "    <!--[if !mso]><!-- -->" +
            "    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
            "    <style>" +
            "      @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?family=Open+Sans'); }" +
            "    </style>" +
            "    <!--<![endif]-->" +
            "    <style>" +
            "      table { color: inherit; }" +
            "    </style>" +
            "  </head>" +
            "  <body style=\"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=\"0\" marginwidth=\"0\" id=\"dbx-email-body\">" +
            "    <div style=\"max-width: 600px !important; padding: 4px;\">" +
            "      <table cellpadding=\"0\" cellspacing=\"0\" style=\"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=\"0\" align==\"center\">" +
            "        <tr>" +
            "          <td align=\"center\">" +
            "            <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"100%\">" +
            "              <tr style=\"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">" +
            "                <td>" +
            "                  <br>" +
            "                  <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + "." +
            "                  <br>" +
            "                  <br>Someone recently requested a password change for your account. If this was you, you can set a new password below." +
            "                  <br>" +
            "                  <br><a style= 'border-radius: 4px; font-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: \"Open Sans\", \"Helvetica Neue\", Arial; margin: 0; display: block; background-color: #007ee6; text-align: center;' href=" + link.AbsoluteUri + ">Change password</a>" +
            "                  <br>If you don't want to change your password or didn't request this, just ignore and delete this message." +
            "                  <br>" +
            "                  <br>To keep your account secure, please don't forward this email to anyone." +
            "                  <br>" +
            "                </td>" +
            "              </tr>" +
            "              <tr>" +
            "                <td height=\"40\"></td>" +
            "              </tr>" +
            "            </table>" +
            "          </td>" +
            "        </tr>" +
            "      </table>" +
            "    </div>" +
            "  </body>" +
            "</html>";
        }

        public static string ApiTemplateConfirmNewUser(AppIssuer issuer, AppUser user, Uri link)
        {
            //use http://rendera.herokuapp.com/ to test template before format...
            //use https://www.buildmystring.com to format template into string that compiles...

            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w3.org/TR/REC-html40/loose.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "  <head>" +
            "    <!--[if !mso]><!-- -->" +
            "    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
            "    <style>" +
            "      @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?f= amily=Open+Sans'); }" +
            "    </style>" +
            "    <!--<![endif]-->" +
            "    <style>" +
            "      table { color: inherit; }" +
            "    </style>" +
            "  </head>" +
            "  <body style=\"font-size: 31px; font-family: 'Open Sans', 'Helvetica Neue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:=#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" marginheight=\"0\" marginwidth=\"0\" id=\"dbx-email-body\">" +
            "    <div style=\"max-width: 600px !important; padding: 4px;\">" +
            "      <table cellpadding=\"0\" cellspacing=\"0\" style=\"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=\"0\" align=\"center\">" +
            "        <tr>" +
            "          <td align=\"center\">" +
            "            <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"100%\">" +
            "              <tr style=\"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">" +
            "                <td>" +
            "                  <br>" +
            "                  <br>Hi " + string.Format("{0} {1}", user.FirstName, user.LastName) + "." +
            "                  <br>" +
            "                  <br>We just need to verify your email address before your sign-up is complete!" +
            "                  <br>" +
            "                  <br><a style=\"border-radius: 4px; font-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: 'Open Sans', 'Helvetica Neue', Arial; margin: 0; display: block; background-color:#007ee6; text-align: center;\" href=" + link.AbsoluteUri + ">Verify your email</a>" +
            "                  <br>" +
            "                </td>" +
            "              <tr>" +
            "              <tr>" +
            "                <td height=\"40\"></td>" +
            "              </tr>" +
            "            </table>" +
            "          </td>" +
            "        </tr>" +
            "      </table>" +
            "    </div>" +
            "  </body>" +
            "</html>";
        }

        #endregion
    }
}
