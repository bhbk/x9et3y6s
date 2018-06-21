using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    public class StartupTest : Startup
    {
        protected static IIdentityContext TestIoC;
        protected static Microsoft.Extensions.Hosting.IHostedService[] TestTasks;
        protected static DatasetHelper TestData;
        protected static StsV2Helper TestStsV2;

        public override void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");
            
            var ioc = new CustomIdentityContext(options);

            sc.AddSingleton<IIdentityContext>(ioc);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainActivityTask(ioc));
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainUsersTask(ioc));

            var sp = sc.BuildServiceProvider();

            TestIoC = (IIdentityContext)sp.GetRequiredService<IIdentityContext>();
            TestTasks = (Microsoft.Extensions.Hosting.IHostedService[])sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            TestData = new DatasetHelper(TestIoC);
        }

        public override void ConfigureServices(IServiceCollection sc)
        {
            base.ConfigureServices(sc);
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            base.Configure(app, env, log);
        }
    }
}
