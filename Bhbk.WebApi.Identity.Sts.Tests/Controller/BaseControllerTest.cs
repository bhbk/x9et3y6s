using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using System.Web.Http;
using Unity;
using Unity.Lifetime;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controller
{
    public class BaseControllerTest : Startup
    {
        protected static UnitOfWork UoW;
        protected static DataSeedHelper Seeds;
        protected static HttpConfiguration HttpConfig;
        protected static TestingHelper Sts;

        public BaseControllerTest()
        {
            UoW = new UnitOfWork(Effort.EntityConnectionFactory.CreateTransient("name=IdentityEntities"));
            Seeds = new DataSeedHelper(UoW);
            Seeds.CreateTestData();

            HttpConfig = new HttpConfiguration();
            Sts = new TestingHelper();
        }

        public override HttpConfiguration ConfigureDependencyInjection()
        {
            UnityContainer container = new UnityContainer();

            container.RegisterType<IUnitOfWork, UnitOfWork>(new TransientLifetimeManager());
            container.RegisterInstance(UoW);
            HttpConfig.DependencyResolver = new CustomDependencyResolver(container);

            return HttpConfig;
        }
    }
}
