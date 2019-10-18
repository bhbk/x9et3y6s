using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Validators
{
    public class ClientValidator
    {
        public static bool Multiple(IEnumerable<string> clients, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            var list = new List<string>();

            foreach (string entry in clients)
                foreach (string entity in entry.Split(','))
                    list.Add(entity.Trim());

            foreach (string entry in list)
                if (validationParameters.ValidAudiences.Contains(entry))
                    return true;

            return false;
        }

        public static bool Single(string client, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (validationParameters.ValidAudiences.Contains(client))
                return true;

            return false;
        }
    }
}
