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
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : GenericRepository<uvw_Roles>
    {
        public RoleRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Roles Create(uvw_Roles entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Roles>().FromSqlRaw("[svc].[usp_Role_Insert]"
                + "@AudienceId, @ActorId, @Name, @Description, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Role_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Roles>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Roles> Create(IEnumerable<uvw_Roles> entities)
        {
            var results = new List<uvw_Roles>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Roles Delete(uvw_Roles entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Roles>().FromSqlRaw("[svc].[usp_Role_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Roles> Delete(IEnumerable<uvw_Roles> entities)
        {
            var results = new List<uvw_Roles>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Roles> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Roles Update(uvw_Roles entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Roles>().FromSqlRaw("[svc].[usp_Role_Update]"
                + "@Id, @AudienceId, @ActorId, @Name, @Description, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Roles> Update(IEnumerable<uvw_Roles> entities)
        {
            var results = new List<uvw_Roles>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}