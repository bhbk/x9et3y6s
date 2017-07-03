using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Store
{
    public class AudienceStore : GenericStore<AppAudience, Guid>
    {
        public ModelFactory Mf;

        public AudienceStore(CustomIdentityDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            Mf = new ModelFactory(context);
        }

        public override AppAudience Create(AppAudience audience)
        {
            var result = _context.AppAudience.Add(audience);
            _context.SaveChanges();

            return result;
        }

        public override bool Delete(Guid audienceId)
        {
            var audience = _context.AppAudience.Where(x => x.Id == audienceId).Single();
            var roles = _context.AppRole.Where(x => x.AudienceId == audienceId);

            _context.AppRole.RemoveRange(roles);
            _context.AppAudience.Remove(audience);
            _context.SaveChanges();

            return true;
            //return Exists(audienceId);
        }

        public override bool Exists(Guid audienceId)
        {
            return _context.AppAudience.Any(x => x.Id == audienceId);
        }

        public bool Exists(string audienceName)
        {
            return _context.AppAudience.Any(x => x.Name == audienceName);
        }

        public override AppAudience FindById(Guid audienceId)
        {
            return _context.AppAudience.Where(x => x.Id == audienceId).SingleOrDefault();
        }

        public AppAudience FindByName(string audienceName)
        {
            return _context.AppAudience.Where(x => x.Name == audienceName).SingleOrDefault();
        }

        public IList<AppAudience> GetAll()
        {
            return _context.AppAudience.ToList();
        }

        public IList<AppRole> GetRoles(Guid audienceId)
        {
            return _context.AppRole.Where(x => x.AudienceId == audienceId).ToList();
        }

        public AppAudience Update(AppAudience audience)
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

            return model;
        }
    }
}
