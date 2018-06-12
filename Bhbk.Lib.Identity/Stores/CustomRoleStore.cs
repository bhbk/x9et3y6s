﻿using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolestorebase-4
//https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers

namespace Bhbk.Lib.Identity.Stores
{
    public class CustomRoleStore : RoleStore<AppRole, AppDbContext, Guid>
    {
        private AppDbContext _context;

        public CustomRoleStore(AppDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public override Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            _context.AppRole.Add(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            _context.AppRole.Remove(role);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<AppRole> FindByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_context.AppRole.Where(x => x.Id.ToString() == id).SingleOrDefault());
        }

        public override Task<AppRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_context.AppRole.Where(x => x.Name == normalizedName).SingleOrDefault());
        }

        public bool Exists(Guid roleId)
        {
            return _context.AppRole.Any(x => x.Id == roleId);
        }

        public bool Exists(string roleName)
        {
            return _context.AppRole.Any(x => x.Name == roleName);
        }

        public IList<AppRole> Get()
        {
            return _context.AppRole.ToList();
        }

        public IEnumerable<AppRole> Get(Expression<Func<AppRole, bool>> filter = null,
            Func<IQueryable<AppRole>, IOrderedQueryable<AppRole>> orderBy = null, string includes = "")
        {
            IQueryable<AppRole> query = _context.AppRole.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            foreach (var include in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include);

            if (orderBy != null)
                return orderBy(query).ToList();

            else
                return query.ToList();
        }

        public IList<AppUser> GetUsersAsync(AppRole role)
        {
            IList<AppUser> result = new List<AppUser>();
            var list = _context.AppUserRole.Where(x => x.RoleId == role.Id).ToList();

            if (list == null)
                throw new InvalidOperationException();

            foreach (AppUserRole entry in list)
                result.Add(_context.AppUser.Where(x => x.Id == entry.UserId).Single());

            return result;
        }

        public override Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var model = _context.AppRole.Where(x => x.Id == role.Id).Single();

            model.Name = role.Name;
            model.Description = role.Description;
            model.Enabled = role.Enabled;
            model.Immutable = role.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}