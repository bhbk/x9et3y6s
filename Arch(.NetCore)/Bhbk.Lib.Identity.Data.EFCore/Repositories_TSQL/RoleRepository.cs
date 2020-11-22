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
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1
     */

    public class RoleRepository : GenericRepository<uvw_Role>
    {
        public RoleRepository(IdentityEntities context)
            : base(context) { }

        public override uvw_Role Create(uvw_Role entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Role>("EXEC @ReturnValue = [svc].[usp_Role_Insert]"
                + "@AudienceId, @Name, @Description, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Role> Create(IEnumerable<uvw_Role> entities)
        {
            var results = new List<uvw_Role>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Role Delete(uvw_Role entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue
            };

            return _context.SqlQuery<uvw_Role>("EXEC @ReturnValue = [svc].[usp_Role_Delete] @Id", pvalues).Single();
        }

        public override IEnumerable<uvw_Role> Delete(IEnumerable<uvw_Role> entities)
        {
            var results = new List<uvw_Role>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Role> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Role>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Role Update(uvw_Role entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("AudienceId", SqlDbType.UniqueIdentifier) { Value = entity.AudienceId },
                new SqlParameter("Name", SqlDbType.NVarChar) { Value = entity.Name },
                new SqlParameter("Description", SqlDbType.NVarChar) { Value = (object)entity.Description ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue
            };

            return _context.SqlQuery<uvw_Role>("EXEC @ReturnValue = [svc].[usp_Role_Update]"
                + "@Id, @AudienceId, @Name, @Description, @IsEnabled, @IsDeletable", pvalues).Single();
        }

        public override IEnumerable<uvw_Role> Update(IEnumerable<uvw_Role> entities)
        {
            var results = new List<uvw_Role>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}