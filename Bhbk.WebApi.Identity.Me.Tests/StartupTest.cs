using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bhbk.WebApi.Identity.Me.Tests
{
    //https://joonasw.net/view/aspnet-core-di-deep-dive
    //http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core
    public class StartupTest : Startup
    {
        protected static IIdentityContext Context;
        protected static DataHelper TestData;

        public override void ConfigureContext(IServiceCollection services)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            Context = new CustomIdentityContext(options);
            TestData = new DataHelper(Context);

            services.AddSingleton<IIdentityContext>(Context);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            base.Configure(app, env, log);
        }
    }
}
