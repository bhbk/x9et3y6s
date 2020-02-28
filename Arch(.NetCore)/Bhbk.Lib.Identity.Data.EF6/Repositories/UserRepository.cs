using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class UserRepository : GenericRepository<uvw_Users>
    {
        public UserRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }

        public override uvw_Users Create(uvw_Users entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.NVarChar) { Value = entity.Email });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = entity.FirstName });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.LastName });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.PhoneNumber });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.Bit) { Value = entity.LockoutEnabled });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.Bit) { Value = entity.HumanBeing });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Users>("[svc].[usp_User_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Users> Create(IEnumerable<uvw_Users> entities)
        {
            var results = new List<uvw_Users>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Users Delete(uvw_Users entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Users>("[svc].[usp_User_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Users> Delete(IEnumerable<uvw_Users> entities)
        {
            var results = new List<uvw_Users>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Users> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Users Update(uvw_Users entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = entity.Email });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.FirstName });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.LastName });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.NVarChar) { Value = entity.PhoneNumber });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.Bit) { Value = entity.LockoutEnabled });
            pvalues.Add(new SqlParameter("@p9", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p10", SqlDbType.Bit) { Value = entity.HumanBeing });
            pvalues.Add(new SqlParameter("@p11", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Users>("[svc].[usp_User_Update]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Users> Update(IEnumerable<uvw_Users> entities)
        {
            var results = new List<uvw_Users>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
