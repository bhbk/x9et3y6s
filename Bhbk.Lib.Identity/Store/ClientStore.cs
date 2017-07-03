using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Store
{
    public class ClientStore : GenericStore<AppClient, Guid>
    {
        public ModelFactory Mf;

        public ClientStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            Mf = new ModelFactory(context);
        }

        public override AppClient Create(AppClient client)
        {
            var result = _context.AppClient.Add(client);
            _context.SaveChanges();

            return result;
        }

        public override bool Delete(Guid clientId)
        {
            var client = _context.AppClient.Where(x => x.Id == clientId).Single();
            var audiences = _context.AppAudience.Where(x => x.ClientId == clientId);
            var roles = _context.AppRole.Where(x => x.AudienceId == audiences.FirstOrDefault().Id);

            _context.AppRole.RemoveRange(roles);
            _context.AppAudience.RemoveRange(audiences);
            _context.AppClient.Remove(client);
            _context.SaveChanges();

            return true;
            //return Exists(clientId);
        }

        public override bool Exists(Guid clientId)
        {
            return _context.AppClient.Any(x => x.Id == clientId);
        }

        public bool Exists(string clientName)
        {
            return _context.AppClient.Any(x => x.Name == clientName);
        }

        public AppClient FindById(Guid clientId)
        {
            return _context.AppClient.Where(x => x.Id == clientId).SingleOrDefault();
        }

        public AppClient FindByName(string clientName)
        {
            return _context.AppClient.Where(x => x.Name == clientName).SingleOrDefault();
        }

        public IList<AppClient> GetAll()
        {
            return _context.AppClient.ToList();
        }

        public IList<AppAudience> GetAudiences(Guid clientId)
        {
            return _context.AppAudience.Where(x => x.ClientId == clientId).ToList();
        }

        public override AppClient Update(AppClient client)
        {
            var model = _context.AppClient.Where(x => x.Id == client.Id).Single();

            model.Name = client.Name;
            model.Description = client.Description;
            model.Enabled = client.Enabled;
            model.Immutable = client.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return model;
        }
    }
}
