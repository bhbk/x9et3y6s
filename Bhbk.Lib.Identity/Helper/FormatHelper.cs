using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;

namespace Bhbk.Lib.Identity.Helper
{
    public class FormatHelper
    {
        //https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        public static bool ValidateUsernameFormat(string username)
        {
            if (!new EmailAddressAttribute().IsValid(username)
                && !username.EndsWith("@local"))
                return false;

            else
                return true;
        }

        public static bool ValidateJwtFormat(string token)
        {
            //check if string is in jwt format.
            var jwt = new JwtSecurityTokenHandler();

            if (jwt.CanReadToken(token))
                return true;
            else
                return false;
        }
    }
}
