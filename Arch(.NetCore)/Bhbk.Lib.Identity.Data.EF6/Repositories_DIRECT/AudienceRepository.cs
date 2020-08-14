using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class AudienceRepository : GenericRepository<tbl_Audience>
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

        public override tbl_Audience Delete(tbl_Audience audience)
        {
            var activity = _context.Set<tbl_Activity>()
                .Where(x => x.AudienceId == audience.Id);

            var refreshes = _context.Set<tbl_Refresh>()
                .Where(x => x.AudienceId == audience.Id);

            var settings = _context.Set<tbl_Setting>()
                .Where(x => x.AudienceId == audience.Id);

            var states = _context.Set<tbl_State>()
                .Where(x => x.AudienceId == audience.Id);

            var roles = _context.Set<tbl_Role>()
                .Where(x => x.AudienceId == audience.Id);

            _context.Set<tbl_Activity>().RemoveRange(activity);
            _context.Set<tbl_Refresh>().RemoveRange(refreshes);
            _context.Set<tbl_Setting>().RemoveRange(settings);
            _context.Set<tbl_State>().RemoveRange(states);
            _context.Set<tbl_Role>().RemoveRange(roles);

            return _context.Set<tbl_Audience>().Remove(audience);
        }

        public override tbl_Audience Update(tbl_Audience audience)
        {
            var entity = _context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id).Single();

            /*
             * only persist certain fields.
             */
            entity.IssuerId = audience.IssuerId;
            entity.Name = audience.Name;
            entity.Description = audience.Description;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Enabled = audience.Enabled;
            entity.Immutable = audience.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
