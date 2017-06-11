using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity.Core.EntityClient;
using System.Web.Http;

namespace Bhbk.WebApi.Identity.Me.Tests.Controller
{
    public class BaseControllerTest : Startup
    {
        private static EntityConnection _connection;
        protected static CustomIdentityDbContext Context;
        protected static UnitOfWork UoW;
        protected static DataSeedHelper Seeds;
        protected static HttpConfiguration HttpConfig;

        public BaseControllerTest()
        {
            _connection = Effort.EntityConnectionFactory.CreateTransient("name=IdentityEntities");
            Context = new CustomIdentityDbContext(_connection);

            UoW = new UnitOfWork(Context);
            UoW.CustomConfigManager.Config.UnitTestRun = true;

            Seeds = new DataSeedHelper(UoW);
            Seeds.CreateTestData();

            HttpConfig = new HttpConfiguration();
        }

        public override HttpConfiguration ConfigureDependencyInjection()
        {
            UnityContainer container = new UnityContainer();

            container.RegisterType<IdentityDbContext<AppUser, AppRole, Guid, AppUserProvider, AppUserRole, AppUserClaim>, CustomIdentityDbContext>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppAudience, Guid>, AudienceRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppClient, Guid>, ClientRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppProvider, Guid>, ProviderRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppRole, Guid>, RoleRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppUser, Guid>, UserRepository>(new TransientLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new TransientLifetimeManager());
            container.RegisterInstance(Context);
            container.RegisterInstance(UoW);
            HttpConfig.DependencyResolver = new CustomDependencyResolver(container);

            return HttpConfig;
        }
    }
}
