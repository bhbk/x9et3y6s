﻿using Bhbk.Lib.DataAccess.EFCore.Repositories;
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
    public class LoginRepository : GenericRepository<uvw_Login>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Login Create(uvw_Login entity)
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

            return _context.Set<uvw_Login>().FromSqlRaw("[svc].[usp_Login_Insert]"
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

                return result.Cast<uvw_Login>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Login> Create(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Login Delete(uvw_Login entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Login>().FromSqlRaw("[svc].[usp_Login_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Login> Delete(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Login> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Login Update(uvw_Login entity)
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

            return _context.Set<uvw_Login>().FromSqlRaw("[svc].[usp_Login_Update]"
                + "@Id, @ActorId, @Name, @Description, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Login> Update(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
