using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Authorize;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Services;
using Bhbk.WebApi.Identity.Me.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Bhbk.WebApi.Identity.Me
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var lib = SearchRoots.ByAssemblyContext("config-lib.json");
            var api = SearchRoots.ByAssemblyContext("config-me.json");

            var conf = new ConfigurationBuilder()
                .SetBasePath(lib.DirectoryName)
                .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                .AddJsonFile(api.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseSqlServer(conf["Databases:IdentityEntities"]);

            sc.AddSingleton(conf);
            sc.AddScoped<IIdentityUnitOfWork<IdentityDbContext>>(x =>
            {
                return new IdentityUnitOfWork(options, InstanceContext.DeployedOrLocal, conf);
            });
            sc.AddSingleton<IHostedService>(new MaintainQuotesTask(sc, conf));
            sc.AddSingleton<IAuthorizationHandler, IdentityAdminsAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
            sc.AddSingleton<IAlertService>(new AlertService(conf, InstanceContext.DeployedOrLocal, new HttpClient()));

            var sp = sc.BuildServiceProvider();
            var uow = sp.GetRequiredService<IIdentityUnitOfWork<IdentityDbContext>>();

            /*
             * only live context allowed to run...
             */

            if (uow.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            var allowedIssuers = conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                .Select(x => x.Value);

            var allowedClients = conf.GetSection("IdentityTenants:AllowedClients").GetChildren()
                .Select(x => x.Value);

            var issuers = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                .Select(x => x.Name + ":" + uow.IssuerRepo.Salt);

            var issuerKeys = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                .Select(x => x.IssuerKey);

            var clients = (uow.ClientRepo.GetAsync(x => allowedClients.Any(y => y == x.Name)).Result)
                .Select(x => x.Name);

            /*
             * check if issuer compatibility enabled. means no env salt.
             */

            if (uow.ConfigRepo.LegacyModeIssuer)
                issuers = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name).Concat(issuers);

#if DEBUG
            /*
             * check if in debug. add value that is hard coded just for that use.
             */

            issuerKeys = issuerKeys.Concat(conf.GetSection("IdentityTenants:AllowedIssuerKeys").GetChildren()
                .Select(x => x.Value));
#endif

            sc.AddLogging(log =>
            {
                log.AddSerilog();
            });
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
            sc.Configure<ForwardedHeadersOptions>(headers =>
            {
                headers.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            sc.AddSwaggerGen(SwaggerOptions.ConfigureSwaggerGen);
        }

        public virtual void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory log)
        {
            //order below is important...
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

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
        }
    }
}