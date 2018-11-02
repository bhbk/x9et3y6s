﻿using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

//http://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core

namespace Bhbk.WebApi.Identity.Sts.Tests
{
    public class StartupTest : Startup
    {
        protected static IConfigurationRoot _conf;
        protected static IIdentityContext<AppDbContext> _uow;
        protected static IHostedService[] _tasks;
        protected static DefaultData _defaults;
        protected static TestData _tests;

        public override void ConfigureContext(IServiceCollection sc)
        {
            var conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            sc.AddSingleton<IConfigurationRoot>(conf);

            /*
             * https://dotnetcoretutorials.com/2017/03/25/net-core-dependency-injection-lifetimes-explained/
             * transient: persist for work. lot of overhead and stateless.
             * scoped: persist for request. default for framework middlewares/controllers.
             * singleton: persist for application execution. thread safe needed for uow/ef context.
             * 
             * order below matters...
             */

            /*
             * keep use singleton below instead of scoped for tests that require uow/ef context persist
             * across multiple requests. need adjustment to tests to rememdy long term. 
             */

            sc.AddSingleton<IIdentityContext<AppDbContext>>(new IdentityContext(options, ContextType.UnitTest));
            sc.AddSingleton<IHostedService>(new MaintainTokensTask(sc));
        }

        public override void ConfigureServices(IServiceCollection sc)
        {
            base.ConfigureServices(sc);

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _uow = (IIdentityContext<AppDbContext>)sp.GetRequiredService<IIdentityContext<AppDbContext>>();
            _tasks = (IHostedService[])sp.GetServices<IHostedService>();
        }

        public override void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory log)
        {
            base.Configure(app, env, log);

            _defaults = new DefaultData(_uow);
            _tests = new TestData(_uow);
        }
    }
}
