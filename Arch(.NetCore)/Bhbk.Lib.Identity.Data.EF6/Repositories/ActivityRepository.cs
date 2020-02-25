using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class ActivityRepository : GenericRepository<uvw_Activities>
    {
        public ActivityRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override uvw_Activities Create(uvw_Activities entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = entity.ActivityType });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = (object)entity.TableName ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = (object)entity.KeyValues ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = (object)entity.OriginalValues ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.NVarChar) { Value = (object)entity.CurrentValues ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Activities>(Constants.SqlSprocActivityInsert +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8", pvalues.ToArray()).Single();
        }

        public override uvw_Activities Delete(uvw_Activities entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Activities>(Constants.SqlSprocActivityDelete +
                "@p0", pvalues.ToArray()).Single();
        }
    }
}
