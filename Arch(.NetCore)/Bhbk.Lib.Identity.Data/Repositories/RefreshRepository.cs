using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.Repositories
{
    public class RefreshRepository : GenericRepository<uvw_Refresh>
    {
        public RefreshRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Refresh Create(uvw_Refresh entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("RefreshValue", SqlDbType.NVarChar) { Value = entity.RefreshValue },
                new SqlParameter("RefreshType", SqlDbType.NVarChar) { Value = entity.RefreshType },
                new SqlParameter("IssuerUtc", SqlDbType.DateTimeOffset) { Value = entity.IssuedUtc },
                new SqlParameter("ValueFromUtc", SqlDbType.DateTimeOffset) { Value = entity.ValidFromUtc },
                new SqlParameter("ValueToUtc", SqlDbType.DateTimeOffset) { Value = entity.ValidToUtc },
                rvalue
            };

            return _context.SqlQuery<uvw_Refresh>("EXEC @ReturnValue = [svc].[usp_Refresh_Insert] "
                + "@IssuerId, @AudienceId, @UserId, @RefreshValue, @RefreshType, @IssuerUtc, @ValueFromUtc, @ValueToUtc", pvalues).Single();
        }

        public override IEnumerable<uvw_Refresh> Create(IEnumerable<uvw_Refresh> entities)
        {
            var results = new List<uvw_Refresh>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Refresh Delete(uvw_Refresh entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Refresh>("EXEC @ReturnValue = [svc].[usp_Refresh_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Refresh> Delete(IEnumerable<uvw_Refresh> entities)
        {
            var results = new List<uvw_Refresh>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Refresh> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Refresh>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Refresh Update(uvw_Refresh entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Refresh> Update(IEnumerable<uvw_Refresh> entities)
        {
            throw new NotImplementedException();
        }
    }
}
