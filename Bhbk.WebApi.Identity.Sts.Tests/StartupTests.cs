using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Hosting.Options;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.WebApi.Identity.Sts.Tests
{
    [CollectionDefinition("StsTestsCollection")]
    public class StartupTestCollection : ICollectionFixture<StartupTests> { }

    public class StartupTests : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.UnitTest);
            var mapper = new MapperConfiguration(x => x.AddProfile<MapperProfile>()).CreateMapper();

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
                    var sandbox = new UoWService(conf, instance);
                    new DefaultData(sandbox, mapper).CreateAsync().Wait();

                    return sandbox;
                });
                sc.AddSingleton<IHostedService, MaintainRefreshesTask>();
                sc.AddSingleton<IHostedService, MaintainStatesTask>();

                /*
                 * do not use dependency injection for unit of work below. is used 
                 * only for owin authentication configuration.
                 */

                var owin = new UoWService(conf, instance);
                new DefaultData(owin, mapper).CreateAsync().Wait();

                /*
                 * only test context allowed to run...
                 */

                if (owin.InstanceType != InstanceContext.UnitTest)
                    throw new NotSupportedException();

                var issuers = (owin.IssuerRepo.GetAsync().Result)
                    .Select(x => x.Name + ":" + owin.IssuerRepo.Salt);

                var issuerKeys = (owin.IssuerRepo.GetAsync().Result)
                    .Select(x => x.IssuerKey);

                var clients = (owin.ClientRepo.GetAsync().Result)
                    .Select(x => x.Name);

                /*
                 * check if issuer compatibility enabled. means no env salt.
                 */

                var legacyIssuer = (owin.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiSettingGlobalLegacyIssuer)).Result.Single();

                if (bool.Parse(legacyIssuer.ConfigValue))
                    issuers = (owin.IssuerRepo.GetAsync().Result)
                        .Select(x => x.Name).Concat(issuers);

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
                        ValidAudiences = clients.ToArray(),
                        AudienceValidator = Bhbk.Lib.Identity.Validators.ClientValidator.Multiple,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.Zero,
                    };
                });
                sc.AddMvc(opt =>
                {
                    opt.EnableEndpointRouting = false;
                }).AddNewtonsoftJson(json =>
                {
                    json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

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
                sc.Configure((ForwardedHeadersOptions opt) =>
                {
                    opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });
                //sc.AddSwaggerGen(SwaggerOptions.ConfigureSwaggerGen);
            });

            builder.Configure(app =>
            {
                app.UseForwardedHeaders();
                app.UseStaticFiles();
                app.UseCors(opt => opt
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
                app.UseAuthentication();
                app.UseMvc();

                //app.UseMiddleware<ResourceOwner_Deprecate>();
                //app.UseMiddleware<ResourceOwnerRefresh_Deprecate>();

                //app.UseSwagger(SwaggerOptions.ConfigureSwagger);
                //app.UseSwaggerUI(SwaggerOptions.ConfigureSwaggerUI);
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
