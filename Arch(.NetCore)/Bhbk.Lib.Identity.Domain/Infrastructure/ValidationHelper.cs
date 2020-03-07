using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public class ValidationHelper
    {
        private readonly Regex _number = new Regex(@"[0-9]+");
        private readonly Regex _lower = new Regex(@"[a-z]+");
        private readonly Regex _upper = new Regex(@"[A-Z]+");
        private readonly Regex _special = new Regex(@"\W+");

        public IdentityResult ValidateEmail(string email)
        {
            var errors = new List<IdentityError>();
            var describer = new IdentityErrorDescriber();
            var count = 0;

            if (!IsValidEmail(email))
            {
                errors.Add(new IdentityErrorDescriber().InvalidEmail(email));
                count++;
            }

            if (!email.EndsWith("@local"))
            {
                errors.Add(new IdentityError() { Code = "InvalidEmail", Description = "Email " + email + " is not local." });
                count++;
            }

            if (count > 1)
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;
        }

        public IdentityResult ValidatePassword(string password)
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
                return IdentityResult.Failed(errors.ToArray());

            return IdentityResult.Success;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase);
        }
    }
}
