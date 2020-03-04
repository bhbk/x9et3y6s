using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Factories;
using Bhbk.WebApi.Identity.Me.Controllers;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class BaseControllerTests : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.IntegrationTest);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_DIRECT>()).CreateMapper();

            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton<IConfiguration>(conf);
                sc.AddSingleton<IContextService>(instance);
                sc.AddSingleton<IMapper>(mapper);
                sc.AddScoped<IUnitOfWork, UnitOfWork>(x =>
                {
                    var uow = new UnitOfWork(conf["Databases:IdentityEntities"], instance);
                    new GenerateDefaultData(uow, mapper).Create();

                    return uow;
                });
                sc.AddSingleton<IHostedService, MaintainQuotesTask>();
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
