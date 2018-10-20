using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    public class StartupTest : Startup
    {
        protected static DataHelper _data;

        public override void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            //DRY up contexts across controllers and tasks after made thread safe...
            _ioc = new IdentityContext(options);
            _jwt = new S2SJwtContext(_conf, _ioc.ContextStatus);

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IIdentityContext>(_ioc);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainActivityTask(new IdentityContext(options)));
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainUsersTask(new IdentityContext(options)));
            sc.AddSingleton<IS2SJwtContext>(_jwt);

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _ioc = (IIdentityContext)sp.GetRequiredService<IIdentityContext>();
            _tasks = (Microsoft.Extensions.Hosting.IHostedService[])sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            _jwt = (IS2SJwtContext)sp.GetRequiredService<IS2SJwtContext>();
        }

        public override void ConfigureServices(IServiceCollection sc)
        {
            base.ConfigureServices(sc);
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            _data = new DataHelper(_ioc);

            base.Configure(app, env, log);
        }
    }
}
