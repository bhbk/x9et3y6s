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
    public class AuthActivityRepository : GenericRepository<uvw_AuthActivity>
    {
        public AuthActivityRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_AuthActivity Create(uvw_AuthActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("AudienceID", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId.HasValue ? (object)entity.AudienceId.Value : DBNull.Value },
                new SqlParameter("UserID", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("LoginType", SqlDbType.NVarChar) { Value = entity.LoginType },
                new SqlParameter("LoginOutcome", SqlDbType.NVarChar) { Value = entity.LoginOutcome },
                new SqlParameter("LocalEndpoint", SqlDbType.NVarChar) { Value = (object)entity.LocalEndpoint ?? DBNull.Value },
                new SqlParameter("RemoteEndpoint", SqlDbType.NVarChar) { Value = (object)entity.RemoteEndpoint ?? DBNull.Value },
                rvalue
            };

            return _context.SqlQuery<uvw_AuthActivity>("EXEC @ReturnValue = [svc].[usp_AuthActivity_Insert] "
                + "@AudienceId, @UserId, @LoginType, @LoginOutcome, @LocalEndpoint, @RemoteEndpoint", pvalues).Single();
        }

        public override IEnumerable<uvw_AuthActivity> Create(IEnumerable<uvw_AuthActivity> entities)
        {
            var results = new List<uvw_AuthActivity>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_AuthActivity Delete(uvw_AuthActivity entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new[]
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_AuthActivity>("EXEC @ReturnValue = [svc].[usp_AuthActivity_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_AuthActivity> Delete(IEnumerable<uvw_AuthActivity> entities)
        {
            var results = new List<uvw_AuthActivity>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_AuthActivity> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_AuthActivity>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_AuthActivity Update(uvw_AuthActivity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_AuthActivity> Update(IEnumerable<uvw_AuthActivity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
