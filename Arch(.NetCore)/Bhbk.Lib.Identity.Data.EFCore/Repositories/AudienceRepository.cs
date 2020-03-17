using Bhbk.Lib.Common.Services;
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
    public class AudienceRepository : GenericRepository<uvw_Audiences>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService instance)
            : base(context)
        {
            _clock = new ClockService(instance);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public override uvw_Audiences Create(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@LockoutEnabled", SqlDbType.Bit) { Value = entity.LockoutEnabled },
                new SqlParameter("@LockoutEnd", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value },
                new SqlParameter("@LastLoginSuccess", SqlDbType.DateTime2) { Value = entity.LastLoginSuccess.HasValue ? (object)entity.LastLoginSuccess.Value : DBNull.Value },
                new SqlParameter("@LastLoginFailure", SqlDbType.DateTime2) { Value = entity.LastLoginFailure.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value },
                new SqlParameter("@AccessFailedCount", SqlDbType.Int) { Value = entity.AccessFailedCount },
                new SqlParameter("@AccessSuccessCount", SqlDbType.Int) { Value = entity.AccessSuccessCount },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Audiences>().FromSqlRaw("[svc].[usp_Audience_Insert]"
                + "@IssuerId, @ActorId, @Name, @Description, @LockoutEnabled, @LockoutEnd,"
                + "@LastLoginSuccess, @LastLoginFailure, @AccessFailedCount, @AccessSuccessCount, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Audience_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var result = cmd.ExecuteReader();

                return result.Cast<uvw_Audiences>().AsEnumerable().Single();
            }
            */
        }

        public override IEnumerable<uvw_Audiences> Create(IEnumerable<uvw_Audiences> entities)
        {
            var results = new List<uvw_Audiences>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Audiences Delete(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Audiences>().FromSqlRaw("[svc].[usp_Audience_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Audiences> Delete(IEnumerable<uvw_Audiences> entities)
        {
            var results = new List<uvw_Audiences>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Audiences> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Audiences Update(uvw_Audiences entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("@ActorId", SqlDbType.UniqueIdentifier) { Value = entity.ActorId.HasValue ? (object)entity.ActorId.Value : DBNull.Value },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("@Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("@LockoutEnabled", SqlDbType.Bit) { Value = entity.LockoutEnabled },
                new SqlParameter("@LockoutEnd", SqlDbType.DateTimeOffset) { Value = entity.LockoutEnd.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value },
                new SqlParameter("@LastLoginSuccess", SqlDbType.DateTime2) { Value = entity.LastLoginSuccess.HasValue ? (object)entity.LastLoginSuccess.Value : DBNull.Value },
                new SqlParameter("@LastLoginFailure", SqlDbType.DateTime2) { Value = entity.LastLoginFailure.HasValue ? (object)entity.LockoutEnd.Value : DBNull.Value },
                new SqlParameter("@AccessFailedCount", SqlDbType.Int) { Value = entity.AccessFailedCount },
                new SqlParameter("@AccessSuccessCount", SqlDbType.Int) { Value = entity.AccessSuccessCount },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Audiences>().FromSqlRaw("[svc].[usp_Audience_Update]"
                + "@Id, @IssuerId, @ActorId, @Name, @Description, @LockoutEnabled, @LockoutEnd,"
                + "@LastLoginSuccess, @LastLoginFailure, @AccessFailedCount, @AccessSuccessCount, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Audiences> Update(IEnumerable<uvw_Audiences> entities)
        {
            var results = new List<uvw_Audiences>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
