using Microsoft.AspNetCore.Identity;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public interface IValidationHelper
    {
        public IdentityResult ValidateEmail(string email);
        public IdentityResult ValidatePassword(string password);
    }
}
