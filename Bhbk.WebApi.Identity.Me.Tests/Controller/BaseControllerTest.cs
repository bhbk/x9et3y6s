using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using System.Data.Entity.Core.EntityClient;
using System.Web.Http;

namespace Bhbk.WebApi.Identity.Me.Tests.Controller
{
    public class BaseControllerTest : Startup
    {
        private static EntityConnection _connection;
        protected static CustomIdentityDbContext Context;
        protected static UnitOfWork UoW;
        protected static SeedTestsHelper Seeds;
        protected static HttpConfiguration HttpConfig;

        public BaseControllerTest()
        {
            _connection = Effort.EntityConnectionFactory.CreateTransient("name=IdentityEntities");
            Context = new CustomIdentityDbContext(_connection);
            UoW = new UnitOfWork(Context);
            Seeds = new SeedTestsHelper(UoW);
            HttpConfig = new HttpConfiguration();

            Seeds.CreateTestData();
        }
    }
}
