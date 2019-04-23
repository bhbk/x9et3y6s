using Bhbk.Lib.Identity.Internal.Models;
using System;

namespace Bhbk.Lib.Identity.Internal.Primitives
{
    public class Strings
    {
        #region Attribute Constants

        public const string AttrAuthCodeResponseIDV1 = "code";
        public const string AttrAuthCodeResponseIDV2 = "code";
        public const string AttrAuthCodeIDV1 = "authorization_code";
        public const string AttrAuthCodeIDV2 = "authorization_code";
        public const string AttrImplicitResponseIDV1 = "token";
        public const string AttrImplicitResponseIDV2 = "token";
        public const string AttrIssuerIDV1 = "issuer_id";
        public const string AttrIssuerIDV2 = "issuer";
        public const string AttrClientIDV1 = "client_id";
        public const string AttrClientIDV2 = "client";
        public const string AttrClientSecretIDV1 = "client_secret";
        public const string AttrClientSecretIDV2 = "client_secret";
        public const string AttrDeviceCodeIDV1 = "device_code";
        public const string AttrDeviceCodeIDV2 = "device_code";
        public const string AttrGrantTypeIDV1 = "grant_type";
        public const string AttrGrantTypeIDV2 = "grant_type";
        public const string AttrRedirectUriIDV1 = "redirect_uri";
        public const string AttrRedirectUriIDV2 = "redirect_uri";
        public const string AttrRefreshTokenIDV1 = "refresh_token";
        public const string AttrRefreshTokenIDV2 = "refresh_token";
        public const string AttrResourceOwnerIDV1 = "password";
        public const string AttrResourceOwnerIDV2 = "password";
        public const string AttrUserIDV1 = "username";
        public const string AttrUserIDV2 = "user";

        #endregion

        #region Configuration Constants

        public const string ApiDefaultClientUi = "Bhbk.WebUi.Identity";
        public const string ApiDefaultClientApi = "Bhbk.WebApi.Identity";
        public const string ApiDefaultIssuer = "Bhbk";
        public const string ApiDefaultLogin = "local";
        public const string ApiDefaultLoginKey = "local";
        public const string ApiDefaultLoginName = "local";
        public const string ApiDefaultRoleForAdmin = "(Built-In) Admins";
        public const string ApiDefaultRoleForSystem = "(Built-In) Services";
        public const string ApiDefaultRoleForUser = "(Built-In) Users";
        public const string ApiDefaultAdminUser = "admin@local";
        public const string ApiDefaultAdminUserPassword = "pa$$word01!";
        public const string ApiDefaultAdminUserFirstName = "Administrator";
        public const string ApiDefaultAdminUserLastName = "User";
        public const string ApiDefaultAdminUserPhone = "+99999999999";
        public const string ApiDefaultNormalUser = "user@local";
        public const string ApiDefaultNormalUserPassword = "pa$$word02!";
        public const string ApiDefaultNormalUserFirstName = "Normal";
        public const string ApiDefaultNormalUserLastName = "User";
        public const string ApiDefaultNormalUserPhone = "+99999999999";

        #endregion

        #region Configuration Constants (Unit Tests)

        public const string ApiUnitTestClient = "ClientUnitTests";
        public const string ApiUnitTestClientKey = "ClientUnitTestsKey";
        public const string ApiUnitTestClaim = "ClaimUnitTests";
        public const string ApiUnitTestEmailContent = "EmailUnitTestsContent";
        public const string ApiUnitTestEmailSubject = "EmailUnitTestsSubject";
        public const string ApiUnitTestIssuer = "IssuerUnitTests";
        public const string ApiUnitTestIssuerKey = "IssuerUnitTestsKey";
        public const string ApiUnitTestLogin = "LoginUnitTests";
        public const string ApiUnitTestLoginKey = "LoginUnitTestsKey";
        public const string ApiUnitTestRole = "RoleUnitTests";
        public const string ApiUnitTestTextBody = "TextUnitTestsContent";
        public const string ApiUnitTestUser = "unittestuser1@local";
        public const string ApiUnitTestUserPhone = "+11111111111";
        public const string ApiUnitTestUserPassCurrent = "te$tpa$$word01!";
        public const string ApiUnitTestUserPassNew = "te$tpa$$word01!new";
        public const string ApiUnitTestUri = "UrlUnitTests";
        public const string ApiUnitTestUriLink = "https://app.test.net/1/callback";

        #endregion

        #region Messages

        public const string MsgConfirmEmailSubject = "Confirm Email Address";
        public const string MsgConfirmPasswordSubject = "Confirm Password";
        public const string MsgConfirmPhoneSubject = "Confirm Phone Number";
        public const string MsgConfirmNewUserSubject = "Confirm New User";

        #endregion

        #region Templates

        //https://htmlformatter.com/, https://www.freeformatter.com/java-dotnet-escape.html

        public static string TemplateConfirmEmail(tbl_Users user, Uri link)
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

        public static string TemplateConfirmPassword(tbl_Users user, Uri link)
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

        public static string TemplateConfirmNewUser(tbl_Issuers issuer, tbl_Users user, Uri link)
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

        public static string TemplateImplicit(tbl_Issuers issuer, tbl_Clients client, tbl_Users user, Uri link)
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
            "                  <br>The link will log you into the application named " + client.Name + " automatically." +
            "                  <br>" +
            "                  <br><a style=\"border-radius: 4px; font-size: 15px; color: white; text-decoration: none; padding: 14px 7px 14px 7px; width: 210px; max-width: 210px; font-family: 'Open Sans', 'Helvetica Neue', Arial; margin: 0; display: block; background-color:#007ee6; text-align: center;\" href=" + link.AbsoluteUri + ">Login now</a>" +
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
