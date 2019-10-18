using Bhbk.Lib.Identity.Data.Models;
using CryptoHelper;
using Microsoft.AspNetCore.Identity;

namespace Bhbk.Lib.Identity.Data.Validators
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.ipasswordhasher-1?view=aspnetcore-2.0
    public sealed class PasswordHasher : IPasswordHasher<tbl_Users>
    {
        public string HashPassword(tbl_Users user, string password)
        {
            return Crypto.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(tbl_Users user, string hashedPassword, string providedPassword)
        {
            if (Crypto.VerifyHashedPassword(hashedPassword, providedPassword))
                return PasswordVerificationResult.Success;

            return PasswordVerificationResult.Failed;
        }
    }
}
