using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories
{
    public class SettingRepository : GenericRepository<uvw_Setting>
    {
        public SettingRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Setting Create(uvw_Setting entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId.HasValue ? (object)entity.IssuerId.Value : DBNull.Value },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
            };

            return _context.Set<uvw_Setting>().FromSqlRaw("[svc].[usp_Setting_Insert]"
                + "@IssuerId, @AudienceId, @UserId, @ConfigKey, @ConfigValue, @LockoutEndUtc, @IsDeletable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Setting_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Setting>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Setting> Create(IEnumerable<uvw_Setting> entities)
        {
            var results = new List<uvw_Setting>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Setting Delete(uvw_Setting entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Setting>().FromSqlRaw("[svc].[usp_Setting_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Setting> Delete(IEnumerable<uvw_Setting> entities)
        {
            var results = new List<uvw_Setting>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Setting Update(uvw_Setting entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId.HasValue ? (object)entity.IssuerId.Value : DBNull.Value },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
            };

            return _context.Set<uvw_Setting>().FromSqlRaw("[svc].[usp_Setting_Update]"
                + "@Id, @IssuerId, @AudienceId, @UserId, @ConfigKey, @ConfigValue, @LockoutEndUtc, @IsDeletable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Setting> Update(IEnumerable<uvw_Setting> entities)
        {
            var results = new List<uvw_Setting>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
