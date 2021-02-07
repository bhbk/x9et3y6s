using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Alert.Controllers;
using Bhbk.WebApi.Alert.Tests.TestingTools;
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
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    public class BaseServiceTests : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.SystemTest);
            var map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore>())
                .CreateMapper();

            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton<IConfiguration>(conf);
                sc.AddSingleton<IContextService>(env);
                sc.AddSingleton<IMapper>(map);
                sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
                sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
                sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
                {
                    var uow = new UnitOfWork(conf["Databases:IdentityEntities_EFCore"], env);

                    var data = new DefaultDataFactory(uow);
                    data.CreateSettings();
                    data.CreateUserLogins();
                    data.CreateUserRoles();

                    return uow;
                });
                sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();

                /*
                 * do not use dependency injection for unit of work below. is used 
                 * only for owin authentication configuration.
                 */

                var owin = new UnitOfWork(conf["Databases:IdentityEntities_EFCore"], env);

                var data = new DefaultDataFactory(owin);
                data.CreateIssuers();
                data.CreateAudiences();

                var issuers = owin.Issuers.Get()
                    .Select(x => x.Name + ":" + conf["IdentityTenant:Salt"]);

                var issuerKeys = owin.Issuers.Get()
                    .Select(x => x.IssuerKey);

                var audiences = owin.Audiences.Get()
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
                    opt.AddPolicy(DefaultConstants.OAuth2ROPGrants, humans =>
                    {
                        humans.Requirements.Add(new IdentityUsersAuthorizeRequirement());
                    });
                    opt.AddPolicy(DefaultConstants.OAuth2CCGrants, servers =>
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
