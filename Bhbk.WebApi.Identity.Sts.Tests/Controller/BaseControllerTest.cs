using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity.Core.EntityClient;
using System.Web.Http;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controller
{
    public class BaseControllerTest : Startup
    {
        private static EntityConnection _connection;
        protected static CustomIdentityDbContext Context;
        protected static UnitOfWork UoW;
        protected static DataSeedHelper Seeds;
        protected static HttpConfiguration HttpConfig;
        protected static TestingHelper Sts;

        public BaseControllerTest()
        {
            _connection = Effort.EntityConnectionFactory.CreateTransient("name=IdentityEntities");
            Context = new CustomIdentityDbContext(_connection);
            
            UoW = new UnitOfWork(Context);
            UoW.ConfigMgmt.Tweaks.UnitTestRun = true;

            Seeds = new DataSeedHelper(UoW);
            Seeds.CreateTestData();

            HttpConfig = new HttpConfiguration();
            Sts = new TestingHelper();
        }

        public override HttpConfiguration ConfigureDependencyInjection()
        {
            UnityContainer container = new UnityContainer();

            container.RegisterType<IdentityDbContext<AppUser, AppRole, Guid, AppUserProvider, AppUserRole, AppUserClaim>, CustomIdentityDbContext>(new TransientLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new TransientLifetimeManager());
            container.RegisterInstance(Context);
            container.RegisterInstance(UoW);
            HttpConfig.DependencyResolver = new CustomDependencyResolver(container);

            return HttpConfig;
        }
    }
}
