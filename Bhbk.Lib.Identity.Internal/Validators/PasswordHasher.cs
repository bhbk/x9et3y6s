﻿using Bhbk.Lib.Identity.Internal.EntityModels;
using CryptoHelper;
using Microsoft.AspNetCore.Identity;

namespace Bhbk.Lib.Identity.Internal.Validators
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.ipasswordhasher-1?view=aspnetcore-2.0
    public sealed class PasswordHasher : IPasswordHasher<AppUser>
    {
        public string HashPassword(AppUser user, string password)
        {
            return Crypto.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword)
        {
            if (Crypto.VerifyHashedPassword(hashedPassword, providedPassword))
                return PasswordVerificationResult.Success;

            return PasswordVerificationResult.Failed;
        }
    }
}
