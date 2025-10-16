using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.WebApi.Identity.User.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace Bhbk.Test.Identity.Integration.ControllerTests
{
    public class BaseUserControllerTests : WebApplicationFactory<WebApi.Identity.User.Startup>
    {
        public TestDataSettings TestData { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var seedConf = new ConfigurationBuilder()
                .AddJsonFile("seedsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var seedData = new SeedDataSettings();
            seedConf.GetSection("SeedData").Bind(seedData);

            var testConf = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: false, reloadOnChange: false)
                .Build();

            TestData = new TestDataSettings();
            testConf.GetSection("TestData").Bind(TestData);

            var env = new ContextService(InstanceContext.IntegrationTest);
            var map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton(conf);
                sc.AddSingleton<IContextService>(env);
                sc.AddSingleton(map);
                sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
                {
                    var uow = new UnitOfWork(conf["Databases:IdentityEntities_EF"], env);

                    var data = new DefaultDataFactory(uow, seedData);
                    data.CreateSettings();

                    return uow;
                });
                sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();

                sc.AddControllers()
                     .AddNewtonsoftJson(opt =>
                     {
                         opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                     })
                    /* https://github.com/aspnet/Mvc/issues/5992 */
                    .AddApplicationPart(typeof(BaseController).Assembly);
            });

            builder.Configure(app => { });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder();
        }
    }
}
