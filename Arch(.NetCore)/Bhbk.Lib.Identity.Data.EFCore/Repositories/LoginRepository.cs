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
    public class LoginRepository : GenericRepository<uvw_Logins>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Logins Create(uvw_Logins entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@LoginKey", SqlDbType.NVarChar) { Value = entity.LoginKey },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Logins>().FromSqlRaw("[svc].[usp_Login_Insert]"
                + "@ActorId, @Name, @Description, @LoginKey, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Login_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Logins>().AsEnumerable().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Logins>().FromSqlRaw("[svc].[usp_Login_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Logins>().FromSqlRaw("[svc].[usp_Login_Update]"
                + "@Id, @ActorId, @Name, @Description, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
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
