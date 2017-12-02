using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Managers
{
    public class LoginManager : IGenericManager<LoginModel, Guid>
    {
        public LoginStore Store;

        public LoginManager(LoginStore store)
        {
            if (store == null)
                throw new ArgumentNullException();

            Store = store;
        }

        public async Task<LoginModel> CreateAsync(LoginModel model)
        {
            var login = Store.Mf.Devolve.DoIt(model);

            if (Store.Exists(login.LoginProvider))
                throw new InvalidOperationException();

            var result = Store.Create(login);

            return Store.Mf.Evolve.DoIt(result);
        }

        public async Task<bool> DeleteAsync(Guid loginId)
        {
            if (!Store.Exists(loginId))
                throw new InvalidOperationException();

            return Store.Delete(loginId);
        }

        public async Task<LoginModel> FindByIdAsync(Guid loginId)
        {
            var login = Store.FindById(loginId);

            if (login == null)
                return null;

            return Store.Mf.Evolve.DoIt(login);
        }

        public async Task<LoginModel> FindByNameAsync(string loginName)
        {
            var login = Store.FindByName(loginName);

            if (login == null)
                return null;

            return Store.Mf.Evolve.DoIt(login);
        }

        public async Task<IList<LoginModel>> GetListAsync()
        {
            IList<LoginModel> result = new List<LoginModel>();
            var logins = Store.GetAll();

            foreach (AppLogin login in logins)
                result.Add(Store.Mf.Evolve.DoIt(login));

            return result;
        }

        public async Task<IList<UserModel>> GetUsersListAsync(Guid loginId)
        {
            IList<UserModel> result = new List<UserModel>();
            var list = Store.GetUsers(loginId);

            foreach (AppUser entry in list)
                result.Add(Store.Mf.Evolve.DoIt(entry));

            return result;
        }

        public async Task<LoginModel> UpdateAsync(LoginModel model)
        {
            var login = Store.Mf.Devolve.DoIt(model);

            if (!Store.Exists(login.Id))
                throw new InvalidOperationException();

            var result = Store.Update(login);

            return Store.Mf.Evolve.DoIt(result);
        }
    }
}
