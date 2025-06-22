using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    public class LoginRepository : GenericRepository<tbl_Login>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public IEnumerable<tbl_User> GetUsersWithLogin(Guid loginId)
        {
            var userIds = _context.Set<tbl_UserLogin>()
                .Where(x => x.LoginId == loginId)
                .Select(x => x.UserId).ToList();
            return _context.Set<tbl_User>().Where(x => userIds.Contains(x.Id)).ToList();
        }
    }
}
