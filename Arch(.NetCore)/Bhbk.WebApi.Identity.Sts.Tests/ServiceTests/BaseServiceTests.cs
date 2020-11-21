using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Identity.Sts.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    public class BaseServiceTests : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.SystemTest);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_DIRECT>()).CreateMapper();

            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton<IConfiguration>(conf);
                sc.AddSingleton<IContextService>(instance);
                sc.AddSingleton<IMapper>(mapper);
                sc.AddSingleton<IAuthorizationHandler, IdentityUserssAuthorize>();
                sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
                sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
                {
                    var uow = new UnitOfWork(conf["Databases:IdentityEntities"], instance);
                    new DefaultDataFactory(uow, mapper).Create();

                    return uow;
                });
                sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();

                /*
                 * do not use dependency injection for unit of work below. is used 
                 * only for owin authentication configuration.
                 */

                var seeds = new UnitOfWork(conf["Databases:IdentityEntities"], instance);
                new DefaultDataFactory(seeds, mapper).Create();

                var issuers = seeds.Issuers.Get()
                    .Select(x => x.Name + ":" + conf["IdentityTenants:Salt"]);

                var issuerKeys = seeds.Issuers.Get()
                    .Select(x => x.IssuerKey);

                var audiences = seeds.Audiences.Get()
                    .Select(x => x.Name);

                sc.AddControllers()
                    .AddNewtonsoftJson(opt =>
                    {
                        opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    })
                    //https://github.com/aspnet/Mvc/issues/5992
                    .AddApplicationPart(typeof(BaseController).Assembly);
                sc.AddCors();
                sc.AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(jwt =>
                {
                    jwt.IncludeErrorDetails = true;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        //AuthenticationType = "JWT:" + instance.InstanceType.ToString(),
                        //ValidTypes = new List<string>() { "JWT:" + instance.InstanceType.ToString() },
                        ValidIssuers = issuers.ToArray(),
                        IssuerSigningKeys = issuerKeys.Select(x => new SymmetricSecurityKey(Encoding.Unicode.GetBytes(x))).ToArray(),
                        ValidAudiences = audiences.ToArray(),
                        AudienceValidator = AudiencesValidator.Multiple,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireAudience = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                    };
                });
                sc.AddAuthorization(opt =>
                {
                    opt.AddPolicy(Constants.DefaultPolicyForHumans, humans =>
                    {
                        humans.Requirements.Add(new IdentityUsersAuthorizeRequirement());
                    });
                    opt.AddPolicy(Constants.DefaultPolicyForServices, servers =>
                    {
                        servers.Requirements.Add(new IdentityServicesAuthorizeRequirement());
                    });
                });
                sc.AddSwaggerGen(opt =>
                {
                    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Reference", Version = "v1" });
                });
                sc.Configure<ForwardedHeadersOptions>(opt =>
                {
                    opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });
            });

            builder.Configure(app =>
            {
                app.UseForwardedHeaders();
                app.UseStaticFiles();
                app.UseSwagger(opt =>
                {
                    opt.RouteTemplate = "help/{documentName}/index.json";
                });
                app.UseSwaggerUI(opt =>
                {
                    opt.RoutePrefix = "help";
                    opt.SwaggerEndpoint("v1/index.json", "Reference");
                });
                app.UseRouting();
                app.UseCors(opt => opt
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(opt =>
                {
                    opt.MapControllers();
                });
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder();
        }
    }
}
