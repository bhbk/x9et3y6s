using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using System;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepository<tbl_User>
    {
        private IClockService _clock;

        public UserRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public override tbl_User Create(tbl_User user)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            return _context.Set<tbl_User>().Add(user);
        }

        public override tbl_User Delete(tbl_User user)
        {
            var activity = _context.Set<tbl_Activity>()
                .Where(x => x.UserId == user.Id);

            var refreshes = _context.Set<tbl_Refresh>()
                .Where(x => x.UserId == user.Id);

            var settings = _context.Set<tbl_Setting>()
                .Where(x => x.UserId == user.Id);

            var states = _context.Set<tbl_State>()
                .Where(x => x.UserId == user.Id);

            _context.Set<tbl_Activity>().RemoveRange(activity);
            _context.Set<tbl_Refresh>().RemoveRange(refreshes);
            _context.Set<tbl_Setting>().RemoveRange(settings);
            _context.Set<tbl_State>().RemoveRange(states);

            return _context.Set<tbl_User>().Remove(user);
        }

        public override tbl_User Update(tbl_User user)
        {
            var entity = _context.Set<tbl_User>()
                .Where(x => x.Id == user.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.LockoutEnabled = user.LockoutEnabled;
            entity.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
            entity.LastUpdated = Clock.UtcDateTime;
            entity.Immutable = user.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}