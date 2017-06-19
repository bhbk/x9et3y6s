using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class CustomIdentityValidator : IIdentityValidator<AppUser>
    {
        private CustomIdentityDbContext _context;

        public CustomIdentityValidator(CustomIdentityDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public async Task<IdentityResult> ValidateAsync(AppUser user)
        {
            return await Task.FromResult(IdentityResult.Success);
        }
    }
}
