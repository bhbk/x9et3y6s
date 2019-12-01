using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class ActivityRepository : GenericRepository<uvw_Activities>
    {
        public ActivityRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override uvw_Activities Create(uvw_Activities entity)
        {
            SqlParameterCollection parameters = null;
            parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Input, Value = entity.Id });
            parameters.Add(new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Input, Value = entity.AudienceId });

            var result = _context.Set<uvw_Activities>()
                .FromSqlRaw("[svc].[usp_Activity_Insert] @Id, @AudienceId OUTPUT", parameters).ToList();

            return result.First();
        }

        public override uvw_Activities Update(uvw_Activities entity)
        {
            throw new NotImplementedException();
        }
    }
}
