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
    public class IssuerRepository : GenericRepository<uvw_Issuer>
    {
        public IssuerRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Issuer Create(uvw_Issuer entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("IssuerKey", SqlDbType.NVarChar) { Value = entity.IssuerKey },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Issuer>("EXEC @ReturnValue = [svc].[usp_Issuer_Insert] "
                + "@Name, @Description, @IssuerKey, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Issuer> Create(IEnumerable<uvw_Issuer> entities)
        {
            var results = new List<uvw_Issuer>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Issuer Delete(uvw_Issuer entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Issuer>("EXEC @ReturnValue = [svc].[usp_Issuer_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Issuer> Delete(IEnumerable<uvw_Issuer> entities)
        {
            var results = new List<uvw_Issuer>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Issuer> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Issuer>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Issuer Update(uvw_Issuer entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("IssuerKey", SqlDbType.NVarChar) { Value = entity.IssuerKey },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Issuer>("EXEC @ReturnValue = [svc].[usp_Issuer_Update] "
                + "@Id, @Name, @Description, @IssuerKey, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Issuer> Update(IEnumerable<uvw_Issuer> entities)
        {
            var results = new List<uvw_Issuer>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
