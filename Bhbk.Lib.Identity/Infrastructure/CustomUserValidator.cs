using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public sealed class CustomUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            List<IdentityError> errors = new List<IdentityError>();
            int match = 0;

            if (!new EmailAddressAttribute().IsValid(user.Email)
                && !user.Email.EndsWith("@local"))
            {
                errors.Add(new IdentityError() { Code = null, Description = "Invalid format" });
                match++;
            }

            if (match < 0)
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            else
                return Task.FromResult(IdentityResult.Success);
        }
    }
}
