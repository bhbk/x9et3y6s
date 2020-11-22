using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_TSQL
{
    public class ActivityRepository : GenericRepository<uvw_Activity>
    {
        public ActivityRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Activity Create(uvw_Activity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("AudienceID", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserID", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("ActivityType", SqlDbType.NVarChar) { Value = entity.ActivityType },
                new SqlParameter("TableName", SqlDbType.NVarChar) { Value = (object)entity.TableName ?? DBNull.Value },
                new SqlParameter("KeyValues", SqlDbType.NVarChar) { Value = (object)entity.KeyValues ?? DBNull.Value },
                new SqlParameter("OriginalValues", SqlDbType.NVarChar) { Value = (object)entity.OriginalValues ?? DBNull.Value },
                new SqlParameter("CurrentValues", SqlDbType.NVarChar) { Value = (object)entity.CurrentValues ?? DBNull.Value },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Activity>("EXEC @ReturnValue = [svc].[usp_Activity_Insert]"
                + "@AudienceId, @UserId, @ActivityType, @TableName, @KeyValues, @OriginalValues, @CurrentValues, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Activity> Create(IEnumerable<uvw_Activity> entities)
        {
            var results = new List<uvw_Activity>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Activity Delete(uvw_Activity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Activity>("EXEC @ReturnValue = [svc].[usp_Activity_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Activity> Delete(IEnumerable<uvw_Activity> entities)
        {
            var results = new List<uvw_Activity>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Activity> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Activity>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Activity Update(uvw_Activity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Activity> Update(IEnumerable<uvw_Activity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
