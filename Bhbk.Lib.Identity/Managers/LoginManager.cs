using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    public class LoginManager : IGenericManager<AppLogin, Guid>
    {
        public readonly LoginStore Store;

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

            return await Task.FromResult(Store.Create(login));
        }

        public async Task<bool> DeleteAsync(AppLogin login)
        {
            if (!Store.Exists(login.Id))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.Delete(login));
        }

        public async Task<AppLogin> FindByIdAsync(Guid loginId)
        {
            return await Task.FromResult(Store.FindById(loginId));
        }

        public async Task<AppLogin> FindByNameAsync(string loginName)
        {
            return await Task.FromResult(Store.FindByName(loginName));
        }

        public async Task<IQueryable<AppUser>> GetUsersListAsync(Guid loginId)
        {
            if (!Store.Exists(loginId))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.GetUsers(loginId));
        }

        public async Task<AppLogin> UpdateAsync(AppLogin login)
        {
            if (!Store.Exists(login.Id))
                throw new InvalidOperationException();

            return await Task.FromResult(Store.Update(login));
        }
    }
}
