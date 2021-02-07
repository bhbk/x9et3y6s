using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using System;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories_Tbl
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

        public UserRepository(IdentityEntities context, IContextService env)
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