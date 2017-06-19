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
    public class AudienceStore : GenericStore<AppAudience, Guid>
    {
        private CustomIdentityDbContext _context;
        private ModelFactory _factory;

        public AudienceStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _factory = new ModelFactory(context);
        }

        //TODO - change incoming model to AudienceModel.Create
        public Task Create(AudienceModel.Model audience)
        {
            var model = _factory.Devolve.DoIt(audience);

            _context.AppAudience.Add(model);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task Delete(Guid audienceId)
        {
            var audience = _context.AppAudience.Where(x => x.Id == audienceId).Single();
            var roles = _context.AppRole.Where(x => x.AudienceId == audienceId);

            _context.AppRole.RemoveRange(roles);
            _context.AppAudience.Remove(audience);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        private Task<AudienceModel.Model> FindInternal(AppAudience audience)
        {
            if (audience == null)
                return Task.FromResult<AudienceModel.Model>(null);
            else
            {
                var model = _factory.Evolve.DoIt(audience);

                return Task.FromResult(model);
            }
        }

        public Task<AudienceModel.Model> FindById(Guid audienceId)
        {
            return FindInternal(_context.AppAudience.Where(x => x.Id == audienceId).SingleOrDefault());
        }

        public Task<AudienceModel.Model> FindByName(string audienceName)
        {
            return FindInternal(_context.AppAudience.Where(x => x.Name == audienceName).SingleOrDefault());
        }

        public Task<IList<AudienceModel.Model>> GetAll()
        {
            IList<AudienceModel.Model> result = new List<AudienceModel.Model>();
            var audiences = _context.AppAudience.ToList();

            if (audiences == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppAudience audience in audiences)
                    result.Add(_factory.Evolve.DoIt(audience));

                return Task.FromResult(result);
            }
        }

        public Task<IList<RoleModel.Model>> GetRoles(Guid audienceId)
        {
            IList<RoleModel.Model> result = new List<RoleModel.Model>();
            var roles = _context.AppRole.Where(x => x.AudienceId == audienceId).ToList();

            if (roles == null)
                throw new InvalidOperationException();

            else
            {
                foreach (AppRole role in roles)
                    result.Add(_factory.Evolve.DoIt(role));

                return Task.FromResult(result);
            }
        }

        public override bool Exists(Guid audienceId)
        {
            return _context.AppAudience.Any(x => x.Id == audienceId);
        }

        public Task Update(AudienceModel.Update audience)
        {
            var model = _context.AppAudience.Where(x => x.Id == audience.Id).Single();

            model.ClientId = audience.ClientId;
            model.Name = audience.Name;
            model.Description = audience.Description;
            model.AudienceType = audience.AudienceType;
            model.AudienceKey = audience.AudienceKey;
            model.Enabled = audience.Enabled;
            model.Immutable = audience.Immutable;
            model.LastUpdated = DateTime.Now;

            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
