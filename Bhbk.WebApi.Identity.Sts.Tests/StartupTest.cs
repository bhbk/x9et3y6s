﻿using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Database;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core

namespace Bhbk.WebApi.Identity.Sts.Tests
{
    public class StartupTest : Startup
    {
        protected static Defaults _defaults;
        protected static UnitTests _tests;

        public override void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            //context is not thread safe yet. create new one for each background thread.
            _ioc = new IdentityContext(options, ContextType.UnitTest);

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IIdentityContext>(_ioc);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainTokensTask(new IdentityContext(options, ContextType.UnitTest)));

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _ioc = (IIdentityContext)sp.GetRequiredService<IIdentityContext>();
            _tasks = (Microsoft.Extensions.Hosting.IHostedService[])sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
        }

        public override void ConfigureServices(IServiceCollection sc)
        {
            base.ConfigureServices(sc);
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            _defaults = new Defaults(_ioc);
            _tests = new UnitTests(_ioc);

            base.Configure(app, env, log);
        }
    }
}
