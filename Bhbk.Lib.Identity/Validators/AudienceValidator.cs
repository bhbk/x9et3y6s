using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Validators
{
    public class AudienceValidator
    {
        public static bool Multiple(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            var audienceList = new List<string>();

            foreach (string first in audiences)
                foreach (string second in first.Split(','))
                    audienceList.Add(second.Trim());

            foreach (string entry in audienceList)
                if (validationParameters.ValidAudiences.Contains(entry))
                    return true;

            return false;
        }

        public static bool Single(string audience, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            throw new NotImplementedException();
        }
    }
}
