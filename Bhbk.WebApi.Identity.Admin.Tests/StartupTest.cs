using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    public class StartupTest : Startup
    {
        protected static IIdentityContext IoC;
        protected static DatasetHelper TestData;
        protected static StsV2Helper StsV2;

        public override void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            IoC = new CustomIdentityContext(options);
            TestData = new DatasetHelper(IoC);

            sc.AddSingleton<IIdentityContext>(IoC);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainUsersTask(IoC));
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
