using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

//http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core

namespace Bhbk.WebApi.Identity.Me.Tests
{
    public class StartupTest : Startup
    {
        protected static DefaultData _defaults;
        protected static TestData _tests;
        protected static StsClient _sts;

        public override void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            /*
             * https://dotnetcoretutorials.com/2017/03/25/net-core-dependency-injection-lifetimes-explained/
             * transient: persist for work. lot of overhead and stateless.
             * scoped: persist for request. default for framework middlewares/controllers.
             * singleton: persist for application execution. thread safe needed for uow/ef context.
             */

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IHostedService>(new MaintainQuotesTask(new IdentityContext(options, ContextType.UnitTest)));
            sc.AddScoped<IIdentityContext<AppDbContext>>(x =>
            {
                return new IdentityContext(options, ContextType.UnitTest);
            });

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _tasks = (IHostedService[])sp.GetServices<IHostedService>();
            _uow = (IIdentityContext<AppDbContext>)sp.GetRequiredService<IIdentityContext<AppDbContext>>();

            _defaults = new DefaultData(_uow);
            _tests = new TestData(_uow);
            _sts = new StsClient(_conf, ContextType.IntegrationTest);
        }

        public override void ConfigureServices(IServiceCollection sc)
        {
            base.ConfigureServices(sc);
        }

        public override void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory log)
        {
            base.Configure(app, env, log);
        }
    }
}
