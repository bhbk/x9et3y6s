using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.ipasswordvalidator-1?view=aspnetcore-2.0
    public sealed class PasswordValidator : IPasswordValidator<AppUser>
    {
        private readonly Regex _number = new Regex(@"[0-9]+");
        private readonly Regex _lower = new Regex(@"[a-z]+");
        private readonly Regex _upper = new Regex(@"[A-Z]+");
        private readonly Regex _special = new Regex(@"\W+");

        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            int match = 0;

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

            if (match < 3)
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            else
                return Task.FromResult(IdentityResult.Success);
        }
    }
}
