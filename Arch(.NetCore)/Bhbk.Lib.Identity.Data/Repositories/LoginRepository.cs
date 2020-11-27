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
    public class LoginRepository : GenericRepository<uvw_Login>
    {
        public LoginRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Login Create(uvw_Login entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("LoginKey", SqlDbType.NVarChar) { Value = entity.LoginKey },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Login>("EXEC @ReturnValue = [svc].[usp_Login_Insert] "
                + "@Name, @Description, @LoginKey, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Login> Create(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Login Delete(uvw_Login entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Login>("EXEC @ReturnValue = [svc].[usp_Login_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Login> Delete(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Login> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Login>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Login Update(uvw_Login entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("LoginKey", SqlDbType.NVarChar) { Value = entity.LoginKey },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Login>("EXEC @ReturnValue = [svc].[usp_Login_Update] "
                + "@Id, @Name, @Description, @LoginKey, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Login> Update(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
