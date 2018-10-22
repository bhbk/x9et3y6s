using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public sealed class UserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var errors = new List<IdentityError>();
            var describer = new IdentityErrorDescriber();
            var count = 0;

            if (!IsValidEmail(user.Email))
            {
                errors.Add(new IdentityErrorDescriber().InvalidEmail(user.Email));
                count++;
            }

            if (!user.Email.EndsWith("@local"))
            {
                errors.Add(new IdentityError() { Code = "InvalidEmail", Description = "Email " + user.Email + " is not local." });
                count++;
            }

            if (count > 1)
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            else
                return Task.FromResult(IdentityResult.Success);
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase);
        }
    }
}
