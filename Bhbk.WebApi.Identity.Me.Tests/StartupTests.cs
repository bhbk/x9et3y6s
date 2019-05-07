using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
namespace Bhbk.WebApi.Identity.Me.Tests
{
    [CollectionDefinition("MeTestsCollection")]
    public class StartupTestCollection : ICollectionFixture<StartupTests> { }

    public class StartupTests : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.ConfigureServices(sc =>
            {
                var options = new DbContextOptionsBuilder<_DbContext>()
                    .EnableSensitiveDataLogging();

                InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

                sc.AddSingleton(conf);
                sc.AddSingleton<IAuthorizationHandler, IdentityAdminsAuthorize>();
                sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
                sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
                sc.AddScoped<IUnitOfWork, UnitOfWork>(x =>
                {
                    var sandbox = new UnitOfWork(options, conf, InstanceContext.UnitTest);
                    new DefaultData(sandbox).CreateAsync().Wait();

                    return sandbox;
                });
                sc.AddSingleton<IHostedService, MaintainQuotesTask>();

                /*
                 * do not use dependency injection for unit of work below. is used 
                 * only for owin authentication configuration.
                 */

                var owin = new UnitOfWork(options, conf, InstanceContext.UnitTest);
                new DefaultData(owin).CreateAsync().Wait();

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
                sc.AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(bearer =>
                {
                    bearer.IncludeErrorDetails = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
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
                sc.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                sc.AddMvc().AddJsonOptions(json =>
                {
                    json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
                sc.AddAuthorization(auth =>
                {
                    auth.AddPolicy("AdministratorsPolicy", admins =>
                    {
                        admins.Requirements.Add(new IdentityAdminsAuthorizeRequirement());
                    });
                    auth.AddPolicy("ServicesPolicy", services =>
                    {
                        services.Requirements.Add(new IdentityServicesAuthorizeRequirement());
                    });
                    auth.AddPolicy("UsersPolicy", users =>
                    {
                        users.Requirements.Add(new IdentityUsersAuthorizeRequirement());
                    });
                });
                sc.Configure((ForwardedHeadersOptions headers) =>
                {
                    headers.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });
                sc.AddSwaggerGen(SwaggerOptions.ConfigureSwaggerGen);
            });

            builder.Configure(app =>
            {
                app.UseForwardedHeaders();
                app.UseCors(policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
                app.UseStaticFiles();
                app.UseAuthentication();
                app.UseMvc();
                app.UseSwagger(SwaggerOptions.ConfigureSwagger);
                app.UseSwaggerUI(SwaggerOptions.ConfigureSwaggerUI);
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder();
        }
    }
}
