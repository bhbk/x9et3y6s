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
    public class ClaimRepository : GenericRepository<uvw_Claim>
    {
        public ClaimRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Claim Create(uvw_Claim entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("Type", SqlDbType.NVarChar) { Value = entity.Type },
                new SqlParameter("Value", SqlDbType.NVarChar) { Value = entity.Value },
                new SqlParameter("ValueType", SqlDbType.NVarChar) { Value = entity.ValueType },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Claim>("EXEC @ReturnValue = [svc].[usp_Claim_Insert]"
                + "@IssuerId, @Subject, @Type, @Value, @ValueType, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Claim> Create(IEnumerable<uvw_Claim> entities)
        {
            var results = new List<uvw_Claim>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Claim Delete(uvw_Claim entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Claim>("EXEC @ReturnValue = [svc].[usp_Claim_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Claim> Delete(IEnumerable<uvw_Claim> entities)
        {
            var results = new List<uvw_Claim>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Claim> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Claim>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Claim Update(uvw_Claim entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IssuerId", SqlDbType.UniqueIdentifier) { Value = entity.IssuerId },
                new SqlParameter("Subject", SqlDbType.NVarChar) { Value = entity.Subject },
                new SqlParameter("Type", SqlDbType.NVarChar) { Value = entity.Type },
                new SqlParameter("Value", SqlDbType.NVarChar) { Value = entity.Value },
                new SqlParameter("ValueType", SqlDbType.NVarChar) { Value = entity.ValueType },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Claim>("EXEC @ReturnValue = [svc].[usp_Claim_Update]"
                + "@Id, @IssuerId, @Subject, @Type, @Value, @ValueType, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Claim> Update(IEnumerable<uvw_Claim> entities)
        {
            var results = new List<uvw_Claim>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
