using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
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
        public ModelFactory Mf;

        public ProviderStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            Mf = new ModelFactory(context);
        }

        public override AppProvider Create(AppProvider provider)
        {
            var result = _context.AppProvider.Add(provider);
            _context.SaveChanges();

            return result;
        }

        public override bool Delete(Guid providerId)
        {
            var provider = _context.AppProvider.Where(x => x.Id == providerId).Single();

            _context.AppProvider.Remove(provider);
            _context.SaveChanges();

            return true;
            //return Exists(providerId);
        }

        public override bool Exists(Guid providerId)
        {
            return _context.AppProvider.Any(x => x.Id == providerId);
        }

        public bool Exists(string providerName)
        {
            return _context.AppProvider.Any(x => x.Name == providerName);
        }

        private Task<ProviderModel> Find(AppProvider provider)
        {
            if (provider == null)
                return Task.FromResult<ProviderModel>(null);
            else
                return Task.FromResult(Mf.Evolve.DoIt(provider));
        }

        public AppProvider FindById(Guid providerId)
        {
            return _context.AppProvider.Where(x => x.Id == providerId).SingleOrDefault();
        }

        public AppProvider FindByName(string providerName)
        {
            return _context.AppProvider.Where(x => x.Name == providerName).SingleOrDefault();
        }

        public IList<AppProvider> GetAll()
        {
            return _context.AppProvider.ToList();
        }

        public IList<AppUser> GetUsers(Guid providerId)
        {
            IList<AppUser> result = new List<AppUser>();
            var list = _context.AppUserProvider.Where(x => x.ProviderId == providerId).ToList();

            if (list == null)
                throw new InvalidOperationException();

            foreach (AppUserProvider entry in list)
                result.Add(_context.AppUser.Where(x => x.Id == entry.UserId).Single());

            return result;
        }

        public bool IsRoleInProvider(Guid provider, string roleName)
        {
            var role = _context.AppProvider.Where(x => x.Name == roleName).SingleOrDefault();

            if (role == null)
                throw new ArgumentNullException();

            else if (_context.AppUserProvider.Any(x => x.ProviderId == provider && x.UserId == role.Id))
                return true;

            else
                return false;
        }

        public AppProvider Update(AppProvider provider)
        {
            var model = _context.AppProvider.Find(provider.Id);

            model.Name = provider.Name;
            model.Description = provider.Description;
            model.Enabled = provider.Enabled;
            model.Immutable = provider.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return model;
        }
    }
}
