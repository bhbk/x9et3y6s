using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.WebApi.Identity.Sts.Controllers;
using Bhbk.WebApi.Identity.Sts.Middlewares;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.UnitTest);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton<IConfiguration>(conf);
                sc.AddSingleton<IContextService>(instance);
                sc.AddSingleton<IMapper>(mapper);
                sc.AddSingleton<IAuthorizationHandler, IdentityAdminsAuthorize>();
                sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
                sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
                sc.AddScoped<IUoWService, UoWService>(x =>
                {
                    var uow = new UoWService(conf, instance);
                    new DefaultData(uow, mapper).Create();

                    return uow;
                });
                sc.AddSingleton<IHostedService, MaintainRefreshesTask>();
                sc.AddSingleton<IHostedService, MaintainStatesTask>();
                sc.AddSingleton<IJsonWebTokenFactory, JsonWebTokenFactory>();

                /*
                 * do not use dependency injection for unit of work below. is used 
                 * only for owin authentication configuration.
                 */

                var owin = new UoWService(conf, instance);
                new DefaultData(owin, mapper).Create();

                /*
                 * only test context allowed to run...
                 */

                if (owin.InstanceType != InstanceContext.UnitTest)
                    throw new NotSupportedException();

                var issuers = owin.Issuers.Get()
                    .Select(x => x.Name + ":" + conf["IdentityTenants:Salt"]);

                var issuerKeys = owin.Issuers.Get()
                    .Select(x => x.IssuerKey);

                var clients = owin.Clients.Get()
                    .Select(x => x.Name);

                /*
                 * check if issuer compatibility enabled. means no env salt.
                 */

                var legacyIssuer = owin.Settings.Get(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiSettingGlobalLegacyIssuer).Single();

                if (bool.Parse(legacyIssuer.ConfigValue))
                    issuers = owin.Issuers.Get()
                        .Select(x => x.Name).Concat(issuers);

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
                        AuthenticationType = "JWT:" + instance.InstanceType.ToString(),
                        ValidTypes = new List<string>() { "JWT:" + instance.InstanceType.ToString() },
                        ValidIssuers = issuers.ToArray(),
                        IssuerSigningKeys = issuerKeys.Select(x => new SymmetricSecurityKey(Encoding.Unicode.GetBytes(x))).ToArray(),
                        ValidAudiences = clients.ToArray(),
                        AudienceValidator = Bhbk.Lib.Identity.Validators.ClientValidator.Multiple,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireAudience = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.Zero,
                    };
                });
                sc.AddAuthorization(opt =>
                {
                    opt.AddPolicy("AdministratorsPolicy", admins =>
                    {
                        admins.Requirements.Add(new IdentityAdminsAuthorizeRequirement());
                    });
                    opt.AddPolicy("ServicesPolicy", services =>
                    {
                        services.Requirements.Add(new IdentityServicesAuthorizeRequirement());
                    });
                    opt.AddPolicy("UsersPolicy", users =>
                    {
                        users.Requirements.Add(new IdentityUsersAuthorizeRequirement());
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

                app.UseMiddleware<SandboxMiddleware>();

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
