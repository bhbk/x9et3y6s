using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    public class ProviderStore : GenericStore<AppProvider, Guid>
    {
        private CustomIdentityDbContext _context;
        private ModelFactory _factory;

        public ProviderStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _factory = new ModelFactory(context);
        }

        //TODO - change incoming model to ProviderModel.Create
        public Task Create(ProviderModel.Model provider)
        {
            var model = _factory.Devolve.DoIt(provider);

            _context.AppProvider.Add(model);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task Delete(Guid providerId)
        {
            var provider = _context.AppProvider.Where(x => x.Id == providerId).Single();

            _context.AppProvider.Remove(provider);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        private Task<ProviderModel.Model> FindInternal(AppProvider provider)
        {
            if (provider == null)
                return Task.FromResult<ProviderModel.Model>(null);
            else
            {
                var model = _factory.Evolve.DoIt(provider);

                return Task.FromResult(model);
            }
        }

        public Task<ProviderModel.Model> FindById(Guid providerId)
        {
            return FindInternal(_context.AppProvider.Where(x => x.Id == providerId).SingleOrDefault());
        }

        public Task<ProviderModel.Model> FindByName(string providerName)
        {
            return FindInternal(_context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault());
        }

        public Task<IList<ProviderModel.Model>> GetAll()
        {
            IList<ProviderModel.Model> result = new List<ProviderModel.Model>();
            var providers = _context.AppProvider.ToList();

            if (providers == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppProvider provider in providers)
                    result.Add(_factory.Evolve.DoIt(provider));

                return Task.FromResult(result);
            }
        }

        public Task<IList<UserModel.Model>> GetUsers(Guid providerId)
        {
            IList<UserModel.Model> result = new List<UserModel.Model>();
            var list = _context.AppUserProvider.Where(x => x.ProviderId == providerId).ToList();

            if (list == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppUserProvider entry in list)
                {
                    var user = _context.AppUser.Where(x => x.Id == entry.UserId).Single();

                    result.Add(_factory.Evolve.DoIt(user));
                }

                return Task.FromResult(result);
            }
        }

        public override bool Exists(Guid providerId)
        {
            return _context.AppProvider.Any(x => x.Id == providerId);
        }

        public Task<bool> IsInProvider(Guid provider, string roleName)
        {
            var user = _context.AppProvider.Where(x => x.Name == roleName).SingleOrDefault();

            if (user == null)
                throw new ArgumentNullException();

            else if (_context.AppUserProvider.Any(x => x.ProviderId == provider && x.UserId == user.Id))
                return Task.FromResult(true);

            else
                return Task.FromResult(false);
        }

        public Task Update(ProviderModel.Update provider)
        {
            var model = _context.AppProvider.Find(provider.Id);

            model.Name = provider.Name;
            model.Description = provider.Description;
            model.Enabled = provider.Enabled;
            model.Immutable = provider.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
