using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF.Repositories
{
    public class IssuerRepository : GenericRepository<tbl_Issuer>
    {
        public IssuerRepository(IdentityEntities context)
            : base(context) { }

        public IEnumerable<tbl_Audience> GetAudiencesForIssuer(Guid issuerId)
        {
            return _context.Set<tbl_Audience>().Where(x => x.IssuerId == issuerId).ToList();
        }
    }
}
