using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public sealed class CustomUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var errors = new List<IdentityError>();

            if(!new EmailAddressAttribute().IsValid(user.Email))
            {
                errors.Add(new IdentityError() { Code = null, Description = BaseLib.Statics.MsgUserInvalid });
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            if (!user.Email.EndsWith("@local"))
            {
                errors.Add(new IdentityError() { Code = null, Description = BaseLib.Statics.MsgUserInvalid });
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
