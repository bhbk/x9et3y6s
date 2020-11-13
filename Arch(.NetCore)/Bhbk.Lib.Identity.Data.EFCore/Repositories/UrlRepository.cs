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
    public class UrlRepository : GenericRepository<uvw_Url>
    {
        public UrlRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Url Create(uvw_Url entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@UrlHost", SqlDbType.NVarChar) { Value = (object)entity.UrlHost ?? DBNull.Value },
                new SqlParameter("@UrlPath", SqlDbType.NVarChar) { Value = (object)entity.UrlPath ?? DBNull.Value },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
            };

            return _context.Set<uvw_Url>().FromSqlRaw("[svc].[usp_Url_Insert]"
                + "@AudienceId, @ActorId, @UrlHost, @UrlPath, @IsEnabled, @IsDeletable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Url_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Url>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Url> Create(IEnumerable<uvw_Url> entities)
        {
            var results = new List<uvw_Url>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Url Delete(uvw_Url entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Url>().FromSqlRaw("[svc].[usp_Url_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Url> Delete(IEnumerable<uvw_Url> entities)
        {
            var results = new List<uvw_Url>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Url Update(uvw_Url entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@UrlHost", SqlDbType.NVarChar) { Value = (object)entity.UrlHost ?? DBNull.Value },
                new SqlParameter("@UrlPath", SqlDbType.NVarChar) { Value = (object)entity.UrlPath ?? DBNull.Value },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
            };

            return _context.Set<uvw_Url>().FromSqlRaw("[svc].[usp_Url_Update]"
                + "@Id, @AudienceId, @ActorId, @UrlHost, @UrlPath, @IsEnabled, @IsDeletable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Url> Update(IEnumerable<uvw_Url> entities)
        {
            var results = new List<uvw_Url>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
