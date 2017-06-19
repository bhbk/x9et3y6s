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
    public class ClientStore : GenericStore<AppClient, Guid>
    {
        private CustomIdentityDbContext _context;
        private ModelFactory _factory;

        public ClientStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _factory = new ModelFactory(context);
        }

        //TODO - change incoming model to ClientModel.Create
        public Task Create(ClientModel.Model client)
        {
            var model = _factory.Devolve.DoIt(client);

            _context.AppClient.Add(model);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task Delete(Guid clientId)
        {
            var client = _context.AppClient.Where(x => x.Id == clientId).Single();
            var audiences = _context.AppAudience.Where(x => x.ClientId == clientId);
            var roles = _context.AppRole.Where(x => x.AudienceId == audiences.FirstOrDefault().Id);

            _context.AppRole.RemoveRange(roles);
            _context.AppAudience.RemoveRange(audiences);
            _context.AppClient.Remove(client);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        private Task<ClientModel.Model> FindInternal(AppClient client)
        {
            if (client == null)
                return Task.FromResult<ClientModel.Model>(null);
            else
            {
                var model = _factory.Evolve.DoIt(client);

                return Task.FromResult(model);
            }
        }

        public Task<ClientModel.Model> FindById(Guid clientId)
        {
            return FindInternal(_context.AppClient.Where(x => x.Id == clientId).SingleOrDefault());
        }

        public Task<ClientModel.Model> FindByName(string clientName)
        {
            return FindInternal(_context.AppClient.Where(x => x.Name == clientName).SingleOrDefault());
        }

        public Task<IList<ClientModel.Model>> GetAll()
        {
            IList<ClientModel.Model> result = new List<ClientModel.Model>();
            var clients = _context.AppClient.ToList();

            if (clients == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppClient client in clients)
                    result.Add(_factory.Evolve.DoIt(client));

                return Task.FromResult(result);
            }
        }

        public Task<IList<AudienceModel.Model>> GetAudiences(Guid clientId)
        {
            IList<AudienceModel.Model> result = new List<AudienceModel.Model>();
            var audiences = _context.AppAudience.Where(x => x.ClientId == clientId).ToList();

            if (audiences == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppAudience audience in audiences)
                    result.Add(_factory.Evolve.DoIt(audience));

                return Task.FromResult(result);
            }
        }

        public override bool Exists(Guid clientId)
        {
            return _context.AppClient.Any(x => x.Id == clientId);
        }

        public Task Update(ClientModel.Update client)
        {
            var model = _context.AppClient.Where(x => x.Id == client.Id).Single();

            model.Name = client.Name;
            model.Description = client.Description;
            model.Enabled = client.Enabled;
            model.Immutable = client.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
