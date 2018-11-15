using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
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
using System.Text;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.WebApi.Identity.Sts.Tests
{
    [CollectionDefinition("StsTestCollection")]
    public class StartupTestCollection : ICollectionFixture<StartupTest> { }

    public class StartupTest : WebApplicationFactory<Startup>
    {
        public IConfigurationRoot Conf;
        public IIdentityContext<AppDbContext> UoW;
        public GenerateDefaultData DefaultData;
        public GenerateTestData TestData;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var lib = SearchRoots.ByAssemblyContext("libsettings.json");
            var api = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = new ConfigurationBuilder()
                .SetBasePath(lib.DirectoryName)
                .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                .AddJsonFile(api.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.ConfigureServices((sc) =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .EnableSensitiveDataLogging();

                InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

                sc.AddSingleton(conf);

                /*
                 * keep use singleton below instead of scoped for tests that require uow/ef context persist
                 * across multiple requests. need adjustment to tests to rememdy long term. 
                 */

                sc.AddSingleton<IIdentityContext<AppDbContext>>(new IdentityContext(options, ContextType.UnitTest, conf));
                sc.AddSingleton<IHostedService>(new MaintainTokensTask(sc, conf));

                var sp = sc.BuildServiceProvider();

                Conf = sp.GetRequiredService<IConfigurationRoot>();
                UoW = sp.GetRequiredService<IIdentityContext<AppDbContext>>();

                TestData = new GenerateTestData(UoW);
                TestData.CreateAsync().Wait();

                DefaultData = new GenerateDefaultData(UoW);
                DefaultData.CreateAsync().Wait();

                /*
                 * only test context allowed to run...
                 */

                if (UoW.Situation != ContextType.UnitTest)
                    throw new NotSupportedException();

                var issuers = (UoW.IssuerRepo.GetAsync().Result)
                    .Select(x => x.Name + ":" + UoW.IssuerRepo.Salt);

                var issuerKeys = (UoW.IssuerRepo.GetAsync().Result)
                    .Select(x => x.IssuerKey);

                var clients = (UoW.ClientRepo.GetAsync().Result)
                    .Select(x => x.Name);

                /*
                 * check if issuer compatibility enabled. means no env salt.
                 */

                if (UoW.ConfigRepo.DefaultsCompatibilityModeIssuer)
                    issuers = (UoW.IssuerRepo.GetAsync().Result)
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
                sc.AddMvc();
                sc.AddMvc().AddControllersAsServices();
                sc.AddMvc().AddJsonOptions(json =>
                {
                    json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
                sc.Configure<ForwardedHeadersOptions>(headers =>
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
                app.UseAuthentication();
                app.UseStaticFiles();
                app.UseMvc();

                //app.UseMiddleware<AccessTokenProvider_Deprecate>();
                //app.UseMiddleware<AuthorizationCodeProvider_Deprecate>();
                //app.UseMiddleware<ClientCredentialProvider_Deprecate>();
                //app.UseMiddleware<RefreshTokenProvider_Deprecate>();

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
