using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Primitives.Constants;
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
    public class AudienceRepository : GenericRepository<uvw_Audience>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public uvw_AudienceRole AddRole(uvw_AudienceRole role)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = role.AudienceId },
                new SqlParameter("RoleId", SqlDbType.UniqueIdentifier) { Value = role.RoleId },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = role.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_AudienceRole>("EXEC @ReturnValue = [svc].[usp_AudienceRole_Insert] "
                + "@AudienceId, @RoleId, @IsDeletable", pvalues).Single();
        }

        public override uvw_Audience Create(uvw_Audience role)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = role.IssuerId },
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = role.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)role.Description ?? DBNull.Value },
                new SqlParameter("IsLockedOut", SqlDbType.Bit) { Value = role.IsLockedOut },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = role.IsDeletable },
                new SqlParameter("LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = role.LockoutEndUtc.HasValue ? (object)role.LockoutEndUtc.Value : DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_Audience>("EXEC @ReturnValue = [svc].[usp_Audience_Insert] "
                + "@IssuerId, @Name, @Description, @IsLockedOut, @IsDeletable, @LockoutEndUtc", pvalues).Single();
        }

        public uvw_Audience Create(uvw_Audience audience, string password)
        {
            audience.ConcurrencyStamp = Guid.NewGuid().ToString();
            audience.SecurityStamp = Guid.NewGuid().ToString();

            var create = Create(audience);

            _context.SaveChanges();

            create = SetPassword(create, password);

            return create;
        }

        public override IEnumerable<uvw_Audience> Create(IEnumerable<uvw_Audience> audiences)
        {
            var results = new List<uvw_Audience>();

            foreach (var entity in audiences)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Audience Delete(uvw_Audience audience)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = audience.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Audience>("EXEC @ReturnValue = [svc].[usp_Audience_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Audience> Delete(IEnumerable<uvw_Audience> audiences)
        {
            var results = new List<uvw_Audience>();

            foreach (var entity in audiences)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Audience> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Audience>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public List<Claim> GenerateAccessClaims(uvw_Issuer issuer, uvw_Audience audience)
        {
            var expire = _context.Set<uvw_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.AccessExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));

            var audienceRoles = _context.Set<uvw_Role>()
                .Where(x => _context.Set<uvw_AudienceRole>().Where(x => x.AudienceId == audience.Id).Any()).ToList();

            foreach (var role in audienceRoles.OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

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

        public List<Claim> GenerateRefreshClaims(uvw_Issuer issuer, uvw_Audience audience)
        {
            var expire = _context.Set<uvw_Setting>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.RefreshExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, audience.Id.ToString()));

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

        public bool IsInRole(uvw_Audience audience, uvw_Role role)
        {
            if (_context.Set<uvw_AudienceRole>()
                .Any(x => x.AudienceId == audience.Id && x.RoleId == role.Id))
                return true;

            return false;
        }

        public bool IsPasswordSet(uvw_Audience audience)
        {
            var entity = _context.Set<uvw_Audience>()
                .Where(x => x.Id == audience.Id).Single();

            if (string.IsNullOrEmpty(entity.PasswordHashPBKDF2))
                return false;

            return true;
        }

        public uvw_AudienceRole RemoveRole(uvw_AudienceRole role)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = role.AudienceId },
                new SqlParameter("RoleId", SqlDbType.UniqueIdentifier) { Value = role.RoleId },
                rvalue
            };

            return _context.SqlQuery<uvw_AudienceRole>("EXEC @ReturnValue = [svc].[usp_AudienceRole_Delete] "
                + "@AudienceId, @RoleId", pvalues).Single();
        }

        public uvw_Audience SetPassword(uvw_Audience audience, string password)
        {
            //https://www.google.com/search?q=identity+securitystamp
            if (!_context.Set<uvw_Audience>()
                .Where(x => x.Id == audience.Id && x.SecurityStamp == audience.SecurityStamp)
                .Any())
                throw new InvalidOperationException();

            if (string.IsNullOrEmpty(password))
            {
                audience.PasswordHashPBKDF2 = null;
                audience.PasswordHashSHA256 = null;
            }
            else
            {
                audience.PasswordHashPBKDF2 = PBKDF2.Create(password);
                audience.PasswordHashSHA256 = SHA256.Create(password);
            }

            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = audience.Id },
                new SqlParameter("PasswordHashPBKDF2", SqlDbType.NVarChar) { Value = (object)audience.PasswordHashPBKDF2 ?? DBNull.Value },
                new SqlParameter("PasswordHashSHA256", SqlDbType.NVarChar) { Value = (object)audience.PasswordHashSHA256 ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_Audience>("EXEC @ReturnValue = [svc].[usp_AudiencePassword_Update] "
                + "@Id, @PasswordHashPBKDF2, @PasswordHashSHA256", pvalues).Single();
        }

        public override uvw_Audience Update(uvw_Audience audience)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = audience.Id },
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = audience.IssuerId },
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = audience.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)audience.Description ?? DBNull.Value },
                new SqlParameter("IsLockedOut", SqlDbType.Bit) { Value = audience.IsLockedOut },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = audience.IsDeletable },
                new SqlParameter("LockoutEndUtc", SqlDbType.DateTimeOffset) { Value = audience.LockoutEndUtc.HasValue ? (object)audience.LockoutEndUtc.Value : DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_Audience>("EXEC @ReturnValue = [svc].[usp_Audience_Update] "
                + "@Id, @IssuerId, @Name, @Description, @IsLockedOut, @IsDeletable, @LockoutEndUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_Audience> Update(IEnumerable<uvw_Audience> audiences)
        {
            var results = new List<uvw_Audience>();

            foreach (var entity in audiences)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
