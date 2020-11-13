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

    public class RoleRepository : GenericRepository<uvw_Role>
    {
        public RoleRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Role Create(uvw_Role entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable }
            };

            return _context.Set<uvw_Role>().FromSqlRaw("[svc].[usp_Role_Insert]"
                + "@AudienceId, @ActorId, @Name, @Description, @IsEnabled, @IsDeletable", pvalues.ToArray())
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

                return result.Cast<uvw_Role>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Role> Create(IEnumerable<uvw_Role> entities)
        {
            var results = new List<uvw_Role>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Role Delete(uvw_Role entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Role>().FromSqlRaw("[svc].[usp_Role_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Role> Delete(IEnumerable<uvw_Role> entities)
        {
            var results = new List<uvw_Role>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Role Update(uvw_Role entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable }
            };

            return _context.Set<uvw_Role>().FromSqlRaw("[svc].[usp_Role_Update]"
                + "@Id, @AudienceId, @ActorId, @Name, @Description, @IsEnabled, @IsDeletable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Role> Update(IEnumerable<uvw_Role> entities)
        {
            var results = new List<uvw_Role>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}