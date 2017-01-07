using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public sealed class CustomPasswordValidator : PasswordValidator
    {
        private readonly Regex _number = new Regex(@"[0-9]+");
        private readonly Regex _lower = new Regex(@"[a-z]+");
        private readonly Regex _upper = new Regex(@"[A-Z]+");
        private readonly Regex _special = new Regex(@"\W+");

        public override async Task<IdentityResult> ValidateAsync(string password)
        {
            List<string> error = new List<string>();
            int match = 0;

            if (_number.IsMatch(password))
            {
                error.Add("Missing a number");
                match++;
            }

            if (_lower.IsMatch(password))
            {
                error.Add("Missing a lowercase letter");
                match++;
            }

            if (_upper.IsMatch(password))
            {
                error.Add("Missing a upppercase letter");
                match++;
            }

            if (_special.IsMatch(password))
            {
                error.Add("Missing a special character");
                match++;
            }

            if (match < 3)
                return new IdentityResult(error);
            else
                return await Task.FromResult(IdentityResult.Success);
        }
    }
}
