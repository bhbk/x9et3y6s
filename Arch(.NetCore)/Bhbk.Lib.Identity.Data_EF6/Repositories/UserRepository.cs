using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models;
using System;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1
     */

    public class UserRepository : GenericRepository<E_User>
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
    }
}
