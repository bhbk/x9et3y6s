using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            //context is not thread safe yet. create new one for each background thread.
            _uow = new IdentityContext(options, ContextType.UnitTest);

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IIdentityContext<AppDbContext>>(_uow);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainQuotesTask(new IdentityContext(options, ContextType.UnitTest)));

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _uow = (IIdentityContext<AppDbContext>)sp.GetRequiredService<IIdentityContext<AppDbContext>>();
            _tasks = (Microsoft.Extensions.Hosting.IHostedService[])sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            _defaults = new DefaultData(_uow);
            _tests = new TestData(_uow);
            _sts = new StsClient(_conf, ContextType.IntegrationTest);
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
