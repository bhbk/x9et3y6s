using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class AudienceValidator
    {
        public static bool MultipleAudience(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            var audienceList = new List<string>();

            foreach (string entry in audiences)
            {
                foreach (string audience in entry.Split(','))
                    audienceList.Add(audience.Trim());
            }

            foreach (string audience in audienceList)
                if (validationParameters.ValidAudiences.Contains(audience))
                    return true;

            return false;
        }
    }
}
