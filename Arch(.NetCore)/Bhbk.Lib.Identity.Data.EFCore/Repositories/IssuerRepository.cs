using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class IssuerRepository : GenericRepository<uvw_Issuers>
    {
        public IssuerRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Issuers Create(uvw_Issuers entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@IssuerKey", SqlDbType.NVarChar) { Value = entity.IssuerKey },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Issuers>().FromSqlRaw("[svc].[usp_Issuer_Insert]"
                + "@ActorId, @Name, @Description, @IssuerKey, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Issuer_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_Issuers>().Single();
            }
            */
        }

        public override IEnumerable<uvw_Issuers> Create(IEnumerable<uvw_Issuers> entities)
        {
            var results = new List<uvw_Issuers>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Issuers Delete(uvw_Issuers entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Issuers>().FromSqlRaw("[svc].[usp_Issuer_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Issuers> Delete(IEnumerable<uvw_Issuers> entities)
        {
            var results = new List<uvw_Issuers>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Issuers> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Issuers Update(uvw_Issuers entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@IssuerKey", SqlDbType.NVarChar) { Value = entity.IssuerKey },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Issuers>().FromSqlRaw("[svc].[usp_Issuer_Update]"
                + "@Id, @ActorId, @Name, @Description, @IssuerKey, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Issuers> Update(IEnumerable<uvw_Issuers> entities)
        {
            var results = new List<uvw_Issuers>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
