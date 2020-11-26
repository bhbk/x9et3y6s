using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models_TSQL;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.Repositories_TSQL
{
    public class SettingRepository : GenericRepository<uvw_Setting>
    {
        public SettingRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Setting Create(uvw_Setting entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId.HasValue ? (object)entity.IssuerId.Value : DBNull.Value },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Setting>("EXEC @ReturnValue = [svc].[usp_Setting_Insert] "
                + "@IssuerId, @AudienceId, @UserId, @ConfigKey, @ConfigValue, @IsDeletable", pvalues).Single();
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
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Setting>("EXEC @ReturnValue = [svc].[usp_Setting_Delete] @Id", pvalues).Single();
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

        public override IEnumerable<uvw_Setting> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Setting>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Setting Update(uvw_Setting entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId.HasValue ? (object)entity.IssuerId.Value : DBNull.Value },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Setting>("EXEC @ReturnValue = [svc].[usp_Setting_Update] "
                + "@Id, @IssuerId, @AudienceId, @UserId, @ConfigKey, @ConfigValue, @IsDeletable", pvalues).Single();
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
