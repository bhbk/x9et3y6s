using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    public class LoginManager : IGenericManager<AppLogin, Guid>
    {
        public LoginStore Store;

        public LoginManager(LoginStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<AppLogin> CreateAsync(AppLogin login)
        {
            if (Store.Exists(login.LoginProvider))
                throw new InvalidOperationException();

            return Store.Create(login);
        }

        public async Task<bool> DeleteAsync(Guid loginId)
        {
            if (!Store.Exists(loginId))
                throw new InvalidOperationException();

            return Store.Delete(loginId);
        }

        public async Task<AppLogin> FindByIdAsync(Guid loginId)
        {
            return Store.FindById(loginId);
        }

        public async Task<AppLogin> FindByNameAsync(string loginName)
        {
            return Store.FindByName(loginName);
        }

        public async Task<IList<AppLogin>> GetListAsync()
        {
            return Store.GetAll();
        }

        public async Task<IList<AppUser>> GetUsersListAsync(Guid loginId)
        {
            if (!Store.Exists(loginId))
                throw new InvalidOperationException();

            return Store.GetUsers(loginId);
        }

        public async Task<AppLogin> UpdateAsync(AppLogin login)
        {
            if (!Store.Exists(login.Id))
                throw new InvalidOperationException();

            return Store.Update(login);
        }
    }
}
