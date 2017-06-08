﻿using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Manager
{
    public class CustomProviderManager
    {
        private CustomProviderStore _store;

        public CustomProviderManager(CustomProviderStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            _store = store;
        }

        public Task<IdentityResult> CreateAsync(AppProvider provider)
        {
            if (!_store.IsProviderValid(provider))
            {
                _store.CreateAsync(provider);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> DeleteAsync(AppProvider provider)
        {
            if (_store.IsProviderValid(provider))
            {
                _store.DeleteAsync(provider);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }

        public Task<AppProvider> FindByIdAsync(Guid providerId)
        {
            return _store.FindByIdAsync(providerId);
        }

        public Task<AppProvider> FindByNameAsync(string providerName)
        {
            return _store.FindByNameAsync(providerName);
        }

        public Task<IList<string>> GetUsersAsync(Guid providerId)
        {
            AppProvider result;

            if (_store.IsProviderValid(providerId, out result))
                return _store.GetUsersAsync(result);
            else
                throw new ArgumentNullException();
        }

        public Task<bool> IsInProviderAsync(Guid providerId, string user)
        {
            AppProvider result;

            if (_store.IsProviderValid(providerId, out result))
                return _store.IsInProviderAsync(result, user);
            else
                throw new ArgumentNullException();
        }

        public Task<IdentityResult> UpdateAsync(AppProvider provider)
        {
            if (_store.IsProviderValid(provider))
            {
                _store.UpdateAsync(provider);
                return Task.FromResult(IdentityResult.Success);
            }
            else
                throw new ArgumentNullException();
        }
    }
}
