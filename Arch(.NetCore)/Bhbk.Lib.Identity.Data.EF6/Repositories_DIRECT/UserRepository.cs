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

    public class UserRepository : GenericRepository<tbl_Users>
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

        public override tbl_Users Create(tbl_Users user)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (!user.HumanBeing)
                user.EmailConfirmed = true;

            return _context.Set<tbl_Users>().Add(user);
        }

        public override tbl_Users Delete(tbl_Users user)
        {
            var activity = _context.Set<tbl_Activities>()
                .Where(x => x.UserId == user.Id);

            var refreshes = _context.Set<tbl_Refreshes>()
                .Where(x => x.UserId == user.Id);

            var settings = _context.Set<tbl_Settings>()
                .Where(x => x.UserId == user.Id);

            var states = _context.Set<tbl_States>()
                .Where(x => x.UserId == user.Id);

            _context.Set<tbl_Activities>().RemoveRange(activity);
            _context.Set<tbl_Refreshes>().RemoveRange(refreshes);
            _context.Set<tbl_Settings>().RemoveRange(settings);
            _context.Set<tbl_States>().RemoveRange(states);

            return _context.Set<tbl_Users>().Remove(user);
        }

        public override tbl_Users Update(tbl_Users user)
        {
            var entity = _context.Set<tbl_Users>()
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