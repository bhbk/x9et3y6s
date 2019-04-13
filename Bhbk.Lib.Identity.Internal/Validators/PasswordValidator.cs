using Bhbk.Lib.Identity.Internal.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Validators
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.ipasswordvalidator-1?view=aspnetcore-2.0
     */

    public sealed class PasswordValidator
    {
        private readonly Regex _number = new Regex(@"[0-9]+");
        private readonly Regex _lower = new Regex(@"[a-z]+");
        private readonly Regex _upper = new Regex(@"[A-Z]+");
        private readonly Regex _special = new Regex(@"\W+");

        public Task<IdentityResult> ValidateAsync(tbl_Users user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            int match = 0;
            int minimum = 8;

            if (_number.IsMatch(password))
            {
                errors.Add(new IdentityError() { Code = null, Description = "Missing a number" });
                match++;
            }

            if (_lower.IsMatch(password))
            {
                errors.Add(new IdentityError() { Code = null, Description = "Missing a lowercase letter" });
                match++;
            }

            if (_upper.IsMatch(password))
            {
                errors.Add(new IdentityError() { Code = null, Description = "Missing a uppercase letter" });
                match++;
            }

            if (_special.IsMatch(password))
            {
                errors.Add(new IdentityError() { Code = null, Description = "Missing a special character" });
                match++;
            }

            if (password.Length <= minimum)
            {
                errors.Add(new IdentityError() { Code = null, Description = "Does not meet minimum length" });
                match++;
            }

            if (match < 3 || password.Length <= minimum)
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
