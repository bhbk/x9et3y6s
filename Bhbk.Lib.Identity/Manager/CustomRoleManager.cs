﻿using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    //https://docs.microsoft.com/en-us/aspnet/identity/overview/extensibility/overview-of-custom-storage-providers-for-aspnet-identity
    public class CustomRoleManager : RoleManager<AppRole, Guid>
    {
        private CustomRoleStore _store;

        public CustomRoleManager(CustomRoleStore store)
            : base(store)
        {
            _store = store;
        }

        public override Task<IdentityResult> CreateAsync(AppRole role)
        {
            if (!IsValidRole(role))
            {
                _store.CreateAsync(role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> DeleteAsync(AppRole role)
        {
            if (IsValidRole(role))
            {
                _store.DeleteAsync(role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<IdentityResult> UpdateAsync(AppRole role)
        {
            if (IsValidRole(role))
            {
                _store.UpdateAsync(role);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public override Task<AppRole> FindByIdAsync(Guid roleId)
        {
            return Task.FromResult(_store.Roles.Where(x => x.Id == roleId).SingleOrDefault());
        }

        public override Task<AppRole> FindByNameAsync(string roleName)
        {
            return Task.FromResult(_store.Roles.Where(x => x.Name == roleName).SingleOrDefault());
        }

        public override IQueryable<AppRole> Roles
        {
            get
            {
                return _store.Roles.AsQueryable();
            }
        }

        public override Task<bool> RoleExistsAsync(string roleName)
        {
            return Task.FromResult(_store.Roles.Any(x => x.Name == roleName));
        }

        private bool IsValidRole(AppRole role)
        {
            var result = _store.Roles.Where(x => x.Id == role.Id || x.Name == role.Name).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }
    }
}