using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.WebApi.Identity.Me.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class BaseControllerTests : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.IntegrationTest);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();

            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton<IConfiguration>(conf);
                sc.AddSingleton<IContextService>(instance);
                sc.AddSingleton<IMapper>(mapper);
                sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(conf["Databases:IdentityEntities"], instance);
                });
                sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();

                sc.AddControllers()
                     .AddNewtonsoftJson(opt =>
                     {
                         opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                     })
                    //https://github.com/aspnet/Mvc/issues/5992
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
