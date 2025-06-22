using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Validators
{
    public class AudiencesValidator
    {
        public static bool Multiple(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            var list = new List<string>();

            foreach (string entry in audiences)
                foreach (string entity in entry.Split(','))
                    list.Add(entity.Trim());

            foreach (string entry in list)
                if (validationParameters.ValidAudiences.Contains(entry))
                    return true;

            return false;
        }

        public static bool Single(string audience, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (validationParameters.ValidAudiences.Contains(audience))
                return true;

            return false;
        }
    }
}
