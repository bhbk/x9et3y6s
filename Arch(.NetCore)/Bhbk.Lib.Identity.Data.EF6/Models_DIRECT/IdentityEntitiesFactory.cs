using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Bhbk.Lib.Identity.Data.EF6.Models_DIRECT
{
    //https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6?view=aspnetcore-3.1
    public class IdentityEntitiesFactory : IDbContextFactory<IdentityEntities>
    {
        private DbConnection _connectionContext = null;
        private string _connectionString = null;
        public IdentityEntitiesFactory(DbConnection connectionContext) => _connectionContext = connectionContext;
        public IdentityEntitiesFactory(string connnectionString) => _connectionString = connnectionString;

        public IdentityEntities Create()
        {
            if (_connectionContext != null)
                return new IdentityEntities(_connectionContext);

            else if (_connectionString != null)
                return new IdentityEntities(_connectionString);

            throw new NotImplementedException();
        }
    }

    public partial class IdentityEntities : DbContext
    {
        public IdentityEntities(string connectionString)
            : base(connectionString) { }

        public IdentityEntities(DbConnection connectionContext)
            : base(connectionContext, true) { }
    }
}
