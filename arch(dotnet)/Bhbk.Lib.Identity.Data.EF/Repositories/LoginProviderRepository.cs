using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    public class LoginProviderRepository : GenericRepository<tbl_LoginProvider>
    {
        public LoginProviderRepository(IdentityEntities context)
            : base(context) { }

        public IEnumerable<tbl_User> GetUsersWithLoginProvider(Guid loginProviderId)
        {
            var userIds = _context.Set<tbl_UserLoginProvider>()
                .Where(x => x.LoginProviderId == loginProviderId)
                .Select(x => x.UserId).ToList();
            return _context.Set<tbl_User>().Where(x => userIds.Contains(x.Id)).ToList();
        }
    }
}
