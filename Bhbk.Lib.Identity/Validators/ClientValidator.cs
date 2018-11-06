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
            var clientList = new List<string>();

            foreach (string first in clients)
                foreach (string second in first.Split(','))
                    clientList.Add(second.Trim());

            foreach (string entry in clientList)
                if (validationParameters.ValidAudiences.Contains(entry))
                    return true;

            return false;
        }

        public static bool Single(string client, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            throw new NotImplementedException();
        }
    }
}
