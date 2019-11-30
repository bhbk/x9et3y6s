using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Validators;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class AudienceRepository : GenericRepository<tbl_Audiences>
    {
        private IClockService _clock;
        private readonly PasswordHasher _passwordHasher;
        private readonly PasswordValidator _passwordValidator;

        public AudienceRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance)
        {
            _clock = new ClockService(new ContextService(instance));
            _passwordHasher = new PasswordHasher();
            _passwordValidator = new PasswordValidator();
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public tbl_Audiences AccessFailed(tbl_Audiences client)
        {
            client.LastLoginFailure = Clock.UtcDateTime;
            client.AccessFailedCount++;

            _context.Entry(client).State = EntityState.Modified;

            return _context.Entry(client).Entity;
        }

        public tbl_Audiences AccessSuccess(tbl_Audiences client)
        {
            client.LastLoginSuccess = Clock.UtcDateTime;
            client.AccessSuccessCount++;

            _context.Entry(client).State = EntityState.Modified;

            return _context.Entry(client).Entity;
        }

        public override tbl_Audiences Delete(tbl_Audiences client)
        {
            var activity = _context.Set<tbl_Activities>()
                .Where(x => x.AudienceId == client.Id);

            var refreshes = _context.Set<tbl_Refreshes>()
                .Where(x => x.AudienceId == client.Id);

            var settings = _context.Set<tbl_Settings>()
                .Where(x => x.AudienceId == client.Id);

            var states = _context.Set<tbl_States>()
                .Where(x => x.AudienceId == client.Id);

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.AudienceId == client.Id);

            _context.RemoveRange(activity);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);

            return _context.Remove(client).Entity;
        }

        public List<Claim> GenerateAccessClaims(tbl_Issuers issuer, tbl_Audiences client)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //service identity vs. a user identity
            claims.Add(new Claim(ClaimTypes.System, AudienceType.server.ToString()));

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.tbl_AudienceRoles.Any(y => y.AudienceId == client.Id)).ToList();

            foreach (var role in roles.OrderBy(x => x.Name))
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        public List<Claim> GenerateRefreshClaims(tbl_Issuers issuer, tbl_Audiences client)
        {
            var expire = _context.Set<tbl_Settings>().Where(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingRefreshExpire).Single();

            var claims = new List<Claim>();

            //add lowest common denominators...
            claims.Add(new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()));

            //nonce to enhance entropy
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, AlphaNumeric.CreateString(8), ClaimValueTypes.String));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(Clock.UtcDateTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(Clock.UtcDateTime)
                .AddSeconds(uint.Parse(expire.ConfigValue)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            return claims;
        }

        internal bool InternalSetPassword(tbl_Audiences client, string password)
        {
            if (_passwordValidator == null)
                throw new NotSupportedException();

            var result = _passwordValidator.ValidateAsync(password);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            if (_passwordHasher == null)
                throw new NotSupportedException();

            var hash = _passwordHasher.HashPassword(password);

            if (!InternalSetPasswordHash(client, hash)
                || !InternalSetSecurityStamp(client, Base64.CreateString(32)))
                return false;

            return true;
        }

        internal bool InternalSetPasswordHash(tbl_Audiences client, string hash)
        {
            client.PasswordHash = hash;
            client.LastUpdated = Clock.UtcDateTime;

            _context.Entry(client).State = EntityState.Modified;

            return true;
        }

        internal bool InternalSetSecurityStamp(tbl_Audiences client, string stamp)
        {
            client.SecurityStamp = stamp;
            client.LastUpdated = Clock.UtcDateTime;

            _context.Entry(client).State = EntityState.Modified;

            return true;
        }

        internal PasswordVerificationResult InternalVerifyPassword(tbl_Audiences client, string password)
        {
            if (_passwordHasher == null)
                throw new NotSupportedException();

            if (_passwordHasher.VerifyHashedPassword(client.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return PasswordVerificationResult.Failed;
        }

        public tbl_Audiences SetPassword(tbl_Audiences client, string password)
        {
            InternalSetPassword(client, password);

            return _context.Entry(client).Entity;
        }

        public override tbl_Audiences Update(tbl_Audiences client)
        {
            var entity = _context.Set<tbl_Audiences>()
                .Where(x => x.Id == client.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.IssuerId = client.IssuerId;
            entity.Name = client.Name;
            entity.Description = client.Description;
            entity.AudienceType = client.AudienceType;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Enabled = client.Enabled;
            entity.Immutable = client.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.Update(entity).Entity;
        }

        public bool VerifyPassword(tbl_Audiences client, string password)
        {
            if (InternalVerifyPassword(client, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }
    }
}
