using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity.Core.EntityClient;
using System.Web.Http;

namespace Bhbk.WebApi.Identity.Admin.Tests
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
        public override HttpConfiguration ConfigureDependencyInjection()
        {
            UnityContainer container = new UnityContainer();

            container.RegisterType<IdentityDbContext<AppUser, AppRole, Guid, AppUserLogin, AppUserRole, AppUserClaim>, CustomIdentityDbContext>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppAudience, Guid>, AudienceRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppClient, Guid>, ClientRepository>(new TransientLifetimeManager());
            container.RegisterType<IGenericRepository<AppRealm, Guid>, RealmRepository>(new TransientLifetimeManager());
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
