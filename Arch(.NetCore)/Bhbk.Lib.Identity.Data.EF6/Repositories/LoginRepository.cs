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
    public class LoginRepository : GenericRepository<uvw_Logins>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Logins Create(uvw_Logins entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.NVarChar) { Value = entity.Name });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = entity.LoginKey });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.Bit) { Value = entity.Enabled });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Logins>("[svc].[usp_Login_Insert]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Logins> Create(IEnumerable<uvw_Logins> entities)
        {
            var results = new List<uvw_Logins>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Logins Delete(uvw_Logins entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });

            return _context.Database.SqlQuery<uvw_Logins>("[svc].[usp_Login_Delete]" +
                "@p0", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Logins> Delete(IEnumerable<uvw_Logins> entities)
        {
            var results = new List<uvw_Logins>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Logins> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Logins Update(uvw_Logins entity)
        {
            var pvalues = new List<SqlParameter>();
            pvalues.Add(new SqlParameter("@p0", SqlDbType.UniqueIdentifier) { Value = entity.Id });
            pvalues.Add(new SqlParameter("@p1", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p2", SqlDbType.NVarChar) { Value = entity.Name });
            pvalues.Add(new SqlParameter("@p3", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value });
            pvalues.Add(new SqlParameter("@p4", SqlDbType.NVarChar) { Value = entity.LoginKey });
            pvalues.Add(new SqlParameter("@p5", SqlDbType.Bit) { Value = entity.Enabled });
            pvalues.Add(new SqlParameter("@p6", SqlDbType.DateTime2) { Value = entity.Created });
            pvalues.Add(new SqlParameter("@p7", SqlDbType.DateTime2) { Value = entity.LastUpdated.HasValue ? (object)entity.LastUpdated.Value : DBNull.Value });
            pvalues.Add(new SqlParameter("@p8", SqlDbType.Bit) { Value = entity.Immutable });

            return _context.Database.SqlQuery<uvw_Logins>("[svc].[usp_Login_Update]" +
                "@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8", pvalues.ToArray()).Single();
        }

        public override IEnumerable<uvw_Logins> Update(IEnumerable<uvw_Logins> entities)
        {
            var results = new List<uvw_Logins>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
