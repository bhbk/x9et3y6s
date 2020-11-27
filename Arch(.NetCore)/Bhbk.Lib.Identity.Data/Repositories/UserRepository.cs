using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepository<uvw_User>
    {
        private IClockService _clock;

        public UserRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public uvw_User AccessFailed(uvw_User user)
        {
            user.LastLoginFailureUtc = Clock.UtcDateTime;
            user.AccessFailedCount++;

            return Update(user);
        }

        public uvw_User AccessSuccess(uvw_User user)
        {
            user.LastLoginSuccessUtc = Clock.UtcDateTime;
            user.AccessSuccessCount++;

            return Update(user);
        }

        public uvw_UserClaim AddClaim(uvw_UserClaim claim)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = claim.UserId },
                new SqlParameter("ClaimId", SqlDbType.UniqueIdentifier) { Value = claim.ClaimId },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = claim.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_UserClaim>("EXEC @ReturnValue = [svc].[usp_UserClaim_Insert] "
                + "@UserId, @ClaimId, @IsDeletable", pvalues).Single();
        }

        public uvw_UserLogin AddLogin(uvw_UserLogin login)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = login.UserId },
                new SqlParameter("LoginId", SqlDbType.UniqueIdentifier) { Value = login.LoginId },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = login.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_UserLogin>("EXEC @ReturnValue = [svc].[usp_UserLogin_Insert] "
                + "@UserId, @LoginId, @IsDeletable", pvalues).Single();
        }

        public uvw_UserRole AddRole(uvw_UserRole role)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = role.UserId },
                new SqlParameter("RoleId", SqlDbType.UniqueIdentifier) { Value = role.RoleId },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = role.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_UserRole>("EXEC @ReturnValue = [svc].[usp_UserRole_Insert] "
                + "@UserId, @RoleId, @IsDeletable", pvalues).Single();
        }

        public override uvw_User Create(uvw_User user)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserName", SqlDbType.NVarChar) { Value = user.UserName },
                new SqlParameter("EmailAddress", SqlDbType.NVarChar) { Value = (object)user.EmailAddress ?? DBNull.Value },
                new SqlParameter("FirstName", SqlDbType.NVarChar) { Value = user.FirstName },
                new SqlParameter("LastName", SqlDbType.NVarChar) { Value = user.LastName },
                new SqlParameter("PhoneNumber", SqlDbType.NVarChar) { Value = user.PhoneNumber },
                new SqlParameter("IsHumanBeing", SqlDbType.Bit) { Value = user.IsHumanBeing },
                new SqlParameter("IsLockedOut", SqlDbType.Bit) { Value = user.IsLockedOut },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = user.IsDeletable },
                new SqlParameter("LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = user.LockoutEndUtc.HasValue ? (object)user.LockoutEndUtc.Value : DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Insert] "
                + "@UserName, @EmailAddress, @FirstName, @LastName, @PhoneNumber, @IsHumanBeing, @IsLockedOut, @IsDeletable, @LockoutEndUtc", pvalues).Single();
        }

        public uvw_User Create(uvw_User user, string password)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.IsHumanBeing)
                user.EmailConfirmed = true;

            var create = Create(user);

            _context.SaveChanges();

            create = SetPassword(create, password);

            return create;
        }

        public override IEnumerable<uvw_User> Create(IEnumerable<uvw_User> users)
        {
            var results = new List<uvw_User>();

            foreach (var entity in users)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_User Delete(uvw_User user)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = user.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_User> Delete(IEnumerable<uvw_User> users)
        {
            var results = new List<uvw_User>();

            foreach (var entity in users)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_User> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_User>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        [Obsolete]
        public List<Claim> GenerateAccessClaims(uvw_User user)
        {
            var legacyClaims = _context.Set<uvw_Setting>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var userRoles = _context.Set<uvw_Role>()
                .Where(x => _context.Set<uvw_UserRole>().Where(x => x.UserId == user.Id).Any()).ToList();

            foreach (var role in userRoles.OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<uvw_Claim>()
                .Where(x => _context.Set<uvw_UserClaim>().Where(x => x.UserId == user.Id).Any()).ToList();

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(86400).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateAccessClaims(uvw_Issuer issuer, uvw_User user)
        {
            var expire = _context.Set<uvw_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingAccessExpire).Single();

            var legacyClaims = _context.Set<uvw_Setting>().Where(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (!string.IsNullOrEmpty(user.EmailAddress))
                claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var userRoles = _context.Set<uvw_Role>()
                .Where(x => _context.Set<uvw_UserRole>().Where(x => x.UserId == user.Id).Any()).ToList();

            foreach (var role in userRoles.OrderBy(x => x.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                //check compatibility is enabled. pack claim(s) with old name and new name.
                if (bool.Parse(legacyClaims.ConfigValue))
                    claims.Add(new Claim("role", role.Name, ClaimTypes.Role));
            }

            var userClaims = _context.Set<uvw_Claim>()
                .Where(x => _context.Set<uvw_UserClaim>().Where(x => x.UserId == user.Id).Any()).ToList();

            foreach (var claim in userClaims.OrderBy(x => x.Type))
                claims.Add(new Claim(claim.Type, claim.Value, claim.ValueType));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateRefreshClaims(uvw_Issuer issuer, uvw_User user)
        {
            var expire = _context.Set<uvw_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(Clock.UtcDateTime).AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public bool IsInClaim(uvw_User user, uvw_Claim claim)
        {
            /*
             * TODO need to add check for role based claims...
             */

            if (_context.Set<uvw_UserClaim>()
                .Any(x => x.UserId == user.Id && x.ClaimId == claim.Id))
                return true;

            return false;
        }

        public bool IsInLogin(uvw_User user, uvw_Login login)
        {
            if (_context.Set<uvw_UserLogin>()
                .Any(x => x.UserId == user.Id && x.LoginId == login.Id))
                return true;

            return false;
        }

        public bool IsInRole(uvw_User user, uvw_Role role)
        {
            if (_context.Set<uvw_UserRole>()
                .Any(x => x.UserId == user.Id && x.RoleId == role.Id))
                return true;

            return false;
        }

        public bool IsLockedOut(uvw_User user)
        {
            if (user.IsLockedOut)
            {
                if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc <= DateTime.UtcNow)
                {
                    user.IsLockedOut = false;
                    user.LockoutEndUtc = null;

                    Update(user);

                    return false;
                }
                else
                    return true;
            }
            else
            {
                user.LockoutEndUtc = null;
                Update(user);

                return false;
            }
        }

        public bool IsPasswordSet(uvw_User user)
        {
            var entity = _context.Set<uvw_User>()
                .Where(x => x.Id == user.Id).Single();

            if (string.IsNullOrEmpty(entity.PasswordHashPBKDF2))
                return false;

            return true;
        }

        public uvw_UserClaim RemoveClaim(uvw_UserClaim claim)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = claim.UserId },
                new SqlParameter("ClaimId", SqlDbType.UniqueIdentifier) { Value = claim.ClaimId },
                rvalue
            };

            return _context.SqlQuery<uvw_UserClaim>("EXEC @ReturnValue = [svc].[usp_UserClaim_Delete] "
                + "@UserId, @ClaimId", pvalues).Single();
        }

        public uvw_UserLogin RemoveLogin(uvw_UserLogin login)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = login.UserId },
                new SqlParameter("LoginId", SqlDbType.UniqueIdentifier) { Value = login.LoginId },
                rvalue
            };

            return _context.SqlQuery<uvw_UserLogin>("EXEC @ReturnValue = [svc].[usp_UserLogin_Delete] "
                + "@UserId, @LoginId", pvalues).Single();
        }

        public uvw_UserRole RemoveRole(uvw_UserRole role)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = role.UserId },
                new SqlParameter("RoleId", SqlDbType.UniqueIdentifier) { Value = role.RoleId },
                rvalue
            };

            return _context.SqlQuery<uvw_UserRole>("EXEC @ReturnValue = [svc].[usp_UserRole_Delete] "
                + "@UserId, @RoleId", pvalues).Single();
        }

        public uvw_User SetConfirmedEmail(uvw_User user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            return Update(user);
        }

        public uvw_User SetConfirmedPassword(uvw_User user, bool confirmed)
        {
            user.PasswordConfirmed = confirmed;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            return Update(user);
        }

        public uvw_User SetConfirmedPhoneNumber(uvw_User user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            return Update(user);
        }

        public uvw_User SetImmutable(uvw_User user, bool enabled)
        {
            user.IsDeletable = enabled;
            user.LastUpdatedUtc = Clock.UtcDateTime;

            return Update(user);
        }

        public uvw_User SetPassword(uvw_User user, string password)
        {
            //https://www.google.com/search?q=identity+securitystamp
            if (!_context.Set<uvw_User>()
                .Where(x => x.Id == user.Id && x.SecurityStamp == user.SecurityStamp)
                .Any())
                throw new InvalidOperationException();

            if (string.IsNullOrEmpty(password))
            {
                user.PasswordHashPBKDF2 = null;
                user.PasswordHashSHA256 = null;
            }
            else
            {
                user.PasswordHashPBKDF2 = PBKDF2.Create(password);
                user.PasswordHashSHA256 = SHA256.Create(password);
            }

            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = user.Id },
                new SqlParameter("PasswordHashPBKDF2", SqlDbType.NVarChar) { Value = (object)user.PasswordHashPBKDF2 ?? DBNull.Value },
                new SqlParameter("PasswordHashSHA256", SqlDbType.NVarChar) { Value = (object)user.PasswordHashSHA256 ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_UserPassword_Update] "
                + "@Id, @PasswordHashPBKDF2, @PasswordHashSHA256", pvalues).Single();
        }

        public override uvw_User Update(uvw_User user)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = user.Id },
                new SqlParameter("UserName", SqlDbType.NVarChar) { Value = user.UserName },
                new SqlParameter("EmailAddress", SqlDbType.NVarChar) { Value = user.EmailAddress },
                new SqlParameter("EmailConfirmed", SqlDbType.Bit) { Value = user.EmailConfirmed },
                new SqlParameter("FirstName", SqlDbType.NVarChar) { Value = user.FirstName },
                new SqlParameter("LastName", SqlDbType.NVarChar) { Value = user.LastName },
                new SqlParameter("PhoneNumber", SqlDbType.NVarChar) { Value = user.PhoneNumber },
                new SqlParameter("PhoneNumberConfirmed", SqlDbType.Bit) { Value = user.PhoneNumberConfirmed },
                new SqlParameter("PasswordConfirmed", SqlDbType.Bit) { Value = user.PasswordConfirmed },
                new SqlParameter("IsHumanBeing", SqlDbType.Bit) { Value = user.IsHumanBeing },
                new SqlParameter("IsLockedOut", SqlDbType.Bit) { Value = user.IsLockedOut },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = user.IsDeletable },
                new SqlParameter("LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = user.LockoutEndUtc.HasValue ? (object)user.LockoutEndUtc.Value : DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Update] "
                + "@Id, @UserName, @EmailAddress, @EmailConfirmed, @FirstName, @LastName, @PhoneNumber, @PhoneNumberConfirmed, @PasswordConfirmed, "
                + "@IsHumanBeing, @IsLockedOut, @IsDeletable, @LockoutEndUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_User> Update(IEnumerable<uvw_User> users)
        {
            var results = new List<uvw_User>();

            foreach (var entity in users)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}