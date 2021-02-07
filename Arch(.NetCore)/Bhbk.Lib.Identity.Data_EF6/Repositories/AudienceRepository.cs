using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models;
using System;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories
{
    public class AudienceRepository : GenericRepository<E_Audience>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService env)
            : base(context)
        {
            _clock = new ClockService(env);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }
    }
}
