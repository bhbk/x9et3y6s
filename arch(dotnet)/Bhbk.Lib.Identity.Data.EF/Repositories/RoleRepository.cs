using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     *
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : GenericRepository<tbl_Role>
    {
        public RoleRepository(IdentityEntities context)
            : base(context) { }

        public IEnumerable<tbl_User> GetUsersInRole(Guid roleId)
        {
            var userIds = _context.Set<tbl_UserRole>()
                .Where(x => x.RoleId == roleId)
                .Select(x => x.UserId).ToList();
            return _context.Set<tbl_User>().Where(x => userIds.Contains(x.Id)).ToList();
        }

        public IEnumerable<tbl_Audience> GetAudiencesInRole(Guid roleId)
        {
            var audienceIds = _context.Set<tbl_AudienceRole>()
                .Where(x => x.RoleId == roleId)
                .Select(x => x.AudienceId).ToList();
            return _context.Set<tbl_Audience>().Where(x => audienceIds.Contains(x.Id)).ToList();
        }
    }
}
