﻿using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Interfaces;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Internal.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Repository
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : IGenericRepository<UserCreate, UserModel, Guid>
    {
        private readonly ContextType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly PasswordValidator _pv;
        public readonly UserClaimFactory claimProvider;
        public readonly PasswordHasher passwordHasher;
        public readonly UserValidator userValidator;

        public UserRepository(AppDbContext context, ContextType situation, IConfigurationRoot conf, IMapper mapper)
        {
            _context = context;
            _situation = situation;
            _mapper = mapper;
            _pv = new PasswordValidator();

            claimProvider = new UserClaimFactory(this, conf);
            passwordHasher = new PasswordHasher();
            userValidator = new UserValidator();
        }

        public async Task<bool> AccessFailedAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

            entity.LastLoginFailure = DateTime.Now;
            entity.AccessFailedCount++;

            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> AccessSuccessAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.LastLoginSuccess = DateTime.Now;
            entity.AccessSuccessCount++;

            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<IdentityResult> AddClaimAsync(Guid key, Claim claims)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            var list = new List<Claim>();
            list.Add(claims);

            foreach (Claim entry in list)
            {
                var model = new AppUserClaim()
                {
                    UserId = entity.Id,
                    ClaimType = entry.Type,
                    ClaimValue = entry.Value,
                    ClaimValueType = entry.ValueType
                };

                _context.AppUserClaim.Add(model);
            }

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddLoginAsync(Guid key, UserLoginInfo info)
        {
            var user = _context.AppUser.Where(x => x.Id == key).Single();
            var login = _context.AppLogin.Where(x => x.LoginProvider == info.LoginProvider).Single();

            var result = new AppUserLogin()
            {
                UserId = user.Id,
                LoginId = login.Id,
                LoginProvider = info.LoginProvider,
                ProviderDisplayName = info.ProviderDisplayName,
                ProviderDescription = info.ProviderDisplayName,
                ProviderKey = info.ProviderKey,
                Created = DateTime.Now,
                Enabled = true,
                Immutable = false
            };

            _context.AppUserLogin.Add(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddPasswordAsync(Guid key, string password)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            if (!string.IsNullOrEmpty(entity.PasswordHash))
                throw new InvalidOperationException();

            if (!await UpdatePassword(entity, password))
                return IdentityResult.Failed();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddRefreshTokenAsync(AppUserRefresh refresh)
        {
            var list = _context.AppUserRefresh.Where(x => x.UserId == refresh.UserId
                && x.IssuedUtc > DateTime.UtcNow
                && x.ExpiresUtc < DateTime.UtcNow);

            if (list != null)
                _context.AppUserRefresh.RemoveRange(list);

            _context.AppUserRefresh.Add(refresh);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddToRoleAsync(Guid key, string roleName)
        {
            var user = _context.AppUser.Where(x => x.Id == key).Single();
            var role = _context.AppRole.Where(x => x.Name == roleName).SingleOrDefault();

            var result = new AppUserRole()
            {
                UserId = user.Id,
                RoleId = role.Id,
                Created = DateTime.Now,
                Immutable = false
            };

            _context.AppUserRole.Add(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> AddToRolesAsync(Guid key, IEnumerable<string> roles)
        {
            foreach (string role in roles)
                await AddToRoleAsync(key, role);

            return IdentityResult.Success;
        }

        public async Task<bool> CheckPasswordAsync(Guid key, string password)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            if (await VerifyPasswordAsync(entity, password) != PasswordVerificationResult.Failed)
                return true;

            return false;
        }

        public async Task<int> Count(Expression<Func<UserModel, bool>> predicates = null)
        {
            var query = _context.AppUser.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppUser, bool>>>(predicates);
                return await query.Where(preds).CountAsync();
            }

            return await query.CountAsync();
        }

        internal async Task<AppUser> CreateAsync(AppUser model)
        {
            var check = await userValidator.ValidateAsync(this, model);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            if (!model.HumanBeing)
                model.EmailConfirmed = true;

            return await Task.FromResult(_context.Add(model).Entity);
        }

        public async Task<UserModel> CreateAsync(UserCreate model)
        {
            var entity = _mapper.Map<AppUser>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            return await Task.FromResult(_mapper.Map<UserModel>(create));
        }

        public async Task<UserModel> CreateAsync(UserCreate model, string password)
        {
            var entity = _mapper.Map<AppUser>(model);
            var create = await CreateAsync(entity);

            _context.SaveChanges();

            await UpdatePassword(entity, password);

            return await Task.FromResult(_mapper.Map<UserModel>(create));
        }

        public async Task<bool> DeleteAsync(Guid key)
        {
            if (!await ExistsAsync(key))
                throw new NullReferenceException();

            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

            try
            {
                await Task.FromResult(_context.Remove(entity).Entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            return await Task.FromResult(_context.AppUser.Any(x => x.Id == key));
        }

        public Task<IEnumerable<UserModel>> GetAsync(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserModel>> GetAsync(Expression<Func<UserModel, bool>> predicates = null,
            Expression<Func<UserModel, object>> orders = null,
            Expression<Func<UserModel, object>> includes = null,
            int? skip = null,
            int? take = null)
        {
            var query = _context.AppUser.AsQueryable();

            if (predicates != null)
            {
                var preds = _mapper.MapExpression<Expression<Func<AppUser, bool>>>(predicates);
                query = query.Where(preds);
            }

            if (orders != null)
            {
                var ords = _mapper.MapExpression<Expression<Func<AppUser, object>>>(orders);
                query = query.OrderBy(ords)?
                        .Skip(skip.Value)?
                        .Take(take.Value);
            }

            query = query.Include("AppUserRole.Role");

            //if (includes != null)
            //{
            //    var incs = _mapper.MapExpression<Expression<Func<AppUser, object>>>(includes);
            //    query = query.Include(incs);
            //}

            return await Task.FromResult(_mapper.Map<IEnumerable<UserModel>>(query));
        }

        public async Task<IQueryable<AppUserRefresh>> GetRefreshTokensAsync(Expression<Func<AppUserRefresh, bool>> predicates = null,
            Func<IQueryable<AppUserRefresh>, IQueryable<AppUserRefresh>> orderBy = null,
            Func<IQueryable<AppUserRefresh>, IIncludableQueryable<AppUserRefresh, object>> includes = null)
        {
            var query = _context.AppUserRefresh.AsQueryable();

            if (predicates != null)
                query = query.Where(predicates);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                return await Task.FromResult(orderBy(query));

            return await Task.FromResult(query);
        }

        public async Task<IList<Claim>> GetClaimsAsync(Guid key)
        {
            var result = new List<Claim>();
            var map = _context.AppUserClaim.Where(x => x.UserId == key);

            if (map == null)
                throw new InvalidOperationException();

            foreach (var claim in map)
            {
                var model = new Claim(claim.ClaimType,
                    claim.ClaimValue,
                    claim.ClaimValueType);

                result.Add(model);
            }

            return await Task.FromResult(result);
        }

        public async Task<IList<string>> GetClientsAsync(Guid key)
        {
            var result = (IList<string>)_context.AppClient
                .Join(_context.AppRole, x => x.Id, y => y.ClientId, (client1, role1) => new {
                    ClientId = client1.Id,
                    RoleId = role1.Id
                })
                .Join(_context.AppUserRole, x => x.RoleId, y => y.RoleId, (role2, user2) => new {
                    ClientId = role2.ClientId,
                    UserId = user2.UserId
                })
                .Where(x => x.UserId == key)
                .Select(x => x.ClientId.ToString().ToLower())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<IList<string>> GetLoginsAsync(Guid key)
        {
            var result = (IList<string>)_context.AppLogin
                .Join(_context.AppUserLogin, x => x.Id, y => y.LoginId, (login1, user1) => new {
                    LoginId = login1.Id,
                    UserId = user1.UserId
                })
                .Where(x => x.UserId == key)
                .Select(x => x.LoginId.ToString().ToLower())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<IList<string>> GetRolesAsync(Guid key)
        {
            var result = (IList<string>)_context.AppRole
                .Join(_context.AppUserRole, x => x.Id, y => y.RoleId, (role1, user1) => new {
                    UserId = user1.UserId,
                    RoleId = role1.Id,
                    RoleName = role1.Name
                })
                .Where(x => x.UserId == key)
                .Select(x => x.RoleName.ToString())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        [System.Obsolete]
        public async Task<IList<string>> GetRolesAsync_Deprecate(Guid key)
        {
            var result = (IList<string>)_context.AppRole
                .Join(_context.AppUserRole, x => x.Id, y => y.RoleId, (role1, user1) => new {
                    UserId = user1.UserId,
                    RoleId = role1.Id,
                    RoleName = role1.Name
                })
                .Where(x => x.UserId == key)
                .Select(x => x.RoleId.ToString())
                .Distinct()
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<bool> IsInLoginAsync(Guid key, string loginName)
        {
            var login = _context.AppLogin.Where(x => x.LoginProvider == loginName).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            else if (_context.AppUserLogin.Any(x => x.UserId == key && x.LoginId == login.Id))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsInRoleAsync(Guid key, string roleName)
        {
            var role = _context.AppRole.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else if (_context.AppUserRole.Any(x => x.UserId == key && x.RoleId == role.Id))
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> IsLockedOutAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

            if (entity.LockoutEnabled)
            {
                if (entity.LockoutEnd.HasValue && entity.LockoutEnd <= DateTime.UtcNow)
                {
                    entity.LockoutEnabled = false;
                    entity.LockoutEnd = null;

                    await UpdateAsync(_mapper.Map<UserModel>(entity));

                    return false;
                }
                else
                    return true;
            }
            else
            {
                entity.LockoutEnd = null;
                await UpdateAsync(_mapper.Map<UserModel>(entity));

                return false;
            }
        }

        public async Task<bool> IsPasswordSetAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).SingleOrDefault();

            if (entity == null)
                return await Task.FromResult(false);

            else if (string.IsNullOrEmpty(entity.PasswordHash))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<IdentityResult> RemoveClaimAsync(Guid key, Claim claim)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(claim);

            foreach (Claim entry in claims)
            {
                var result = _context.AppUserClaim.Where(x => x.UserId == key
                    && x.ClaimType == entry.Type
                    && x.ClaimValue == entry.Value).SingleOrDefault();

                if (result == null)
                    throw new ArgumentNullException();

                _context.AppUserClaim.Remove(result);
            }

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveLoginAsync(Guid key, string loginProvider, string providerKey)
        {
            var login = _context.AppUserLogin.Where(x => x.LoginProvider == loginProvider).SingleOrDefault();

            if (login == null)
                throw new ArgumentNullException();

            var result = _context.AppUserLogin.Where(x => x.UserId == key && x.LoginId == login.LoginId).Single();

            _context.AppUserLogin.Remove(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(Guid key, string roleName)
        {
            var role = _context.Roles.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            var result = _context.AppUserRole.Where(x => x.UserId == key && x.RoleId == role.Id).Single();

            _context.AppUserRole.Remove(result);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(Guid key, IEnumerable<string> roles)
        {
            foreach (string role in roles)
                await RemoveFromRoleAsync(key, role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRefreshTokenAsync(Guid key)
        {
            var token = _context.AppUserRefresh.Where(x => x.Id == key);

            if (token == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(token);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemoveRefreshTokensAsync(Guid key)
        {
            var tokens = _context.AppUserRefresh.Where(x => x.UserId == key);

            if (tokens == null)
                throw new ArgumentNullException();

            _context.AppUserRefresh.RemoveRange(tokens);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> RemovePasswordAsync(Guid key)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            await SetPasswordHashAsync(entity, null);

            return IdentityResult.Success;
        }

        public async Task<bool> SetConfirmedEmailAsync(Guid key, bool confirmed)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.EmailConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPasswordAsync(Guid key, bool confirmed)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.PasswordConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetConfirmedPhoneNumberAsync(Guid key, bool confirmed)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.PhoneNumberConfirmed = confirmed;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetImmutableAsync(Guid key, bool enabled)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.Immutable = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetPasswordHashAsync(AppUser entity, string passwordHash)
        {
            entity.PasswordHash = passwordHash;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        internal async Task<bool> SetSecurityStampAsync(AppUser entity, string stamp)
        {
            entity.SecurityStamp = stamp;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(Guid key, bool enabled)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            entity.TwoFactorEnabled = enabled;
            entity.LastUpdated = DateTime.Now;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(true);
        }

        public async Task<UserModel> UpdateAsync(UserModel model)
        {
            var user = _mapper.Map<AppUser>(model);
            var check = await userValidator.ValidateAsync(this, user);

            if (!check.Succeeded)
                throw new InvalidOperationException();

            var entity = _context.AppUser.Where(x => x.Id == user.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.LockoutEnabled = user.LockoutEnabled;
            entity.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            entity.LastUpdated = DateTime.Now;
            entity.Immutable = user.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return await Task.FromResult(_mapper.Map<UserModel>(entity));
        }

        internal async Task<bool> UpdatePassword(AppUser entity, string password)
        {
            if (_pv == null)
                throw new NotSupportedException();

            var result = await _pv.ValidateAsync(this, entity, password);

            if (!result.Succeeded)
                throw new InvalidOperationException();

            if (passwordHasher == null)
                throw new NotSupportedException();

            var hash = passwordHasher.HashPassword(entity, password);

            if (!await SetPasswordHashAsync(entity, hash)
                || !await SetSecurityStampAsync(entity, RandomValues.CreateBase64String(32)))
                return false;

            return true;
        }

        public async Task<bool> UpdatePassword(Guid key, string password)
        {
            var entity = _context.AppUser.Where(x => x.Id == key).Single();

            if (!await UpdatePassword(entity, password))
                return false;

            return true;
        }

        internal async Task<PasswordVerificationResult> VerifyPasswordAsync(AppUser user, string password)
        {
            if (passwordHasher == null)
                throw new NotSupportedException();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed)
                return PasswordVerificationResult.Success;

            return await Task.FromResult(PasswordVerificationResult.Failed);
        }
    }
}