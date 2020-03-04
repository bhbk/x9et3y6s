using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class AudienceRepository : GenericRepository<tbl_Audiences>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public override tbl_Audiences Delete(tbl_Audiences audience)
        {
            var activity = _context.Set<tbl_Activities>()
                .Where(x => x.AudienceId == audience.Id);

            var refreshes = _context.Set<tbl_Refreshes>()
                .Where(x => x.AudienceId == audience.Id);

            var settings = _context.Set<tbl_Settings>()
                .Where(x => x.AudienceId == audience.Id);

            var states = _context.Set<tbl_States>()
                .Where(x => x.AudienceId == audience.Id);

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.AudienceId == audience.Id);

            _context.Set<tbl_Activities>().RemoveRange(activity);
            _context.Set<tbl_Refreshes>().RemoveRange(refreshes);
            _context.Set<tbl_Settings>().RemoveRange(settings);
            _context.Set<tbl_States>().RemoveRange(states);
            _context.Set<tbl_Roles>().RemoveRange(roles);

            return _context.Set<tbl_Audiences>().Remove(audience);
        }

        public override tbl_Audiences Update(tbl_Audiences audience)
        {
            var entity = _context.Set<tbl_Audiences>()
                .Where(x => x.Id == audience.Id).Single();

            /*
             * only persist certain fields.
             */
            entity.IssuerId = audience.IssuerId;
            entity.Name = audience.Name;
            entity.Description = audience.Description;
            entity.AudienceType = audience.AudienceType;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Enabled = audience.Enabled;
            entity.Immutable = audience.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
