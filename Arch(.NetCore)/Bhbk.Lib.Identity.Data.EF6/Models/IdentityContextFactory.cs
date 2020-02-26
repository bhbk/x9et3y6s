using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Bhbk.Lib.Identity.Data.EF6.Models
{
    //https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6?view=aspnetcore-3.1
    public class IdentityContextFactory : IDbContextFactory<IdentityEntities>
    {
        private string _connection;
        public IdentityContextFactory(string connection) => _connection = connection;

        public IdentityEntities Create()
        {
            return new IdentityEntities(_connection);
        }
    }

    public partial class IdentityEntities : DbContext
    {
        public IdentityEntities(string connString)
            : base(connString)
        {

        }
    }
}
