using AutoMapper;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.UnitOfWork;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var lib = SearchRoots.ByAssemblyContext("libsettings.json");
            var api = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = new ConfigurationBuilder()
                .SetBasePath(lib.DirectoryName)
                .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                .AddJsonFile(api.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseSqlServer(conf["Databases:IdentityEntities"]);

            var mapper = new MapperConfiguration(x =>
            {
                x.AddProfile<AutoMapperProfile>();
            }).CreateMapper();

            sc.AddSingleton(mapper);
            sc.AddSingleton(conf);
            sc.AddScoped<IIdentityUnitOfWork<IdentityDbContext>>(x =>
            {
                return new IdentityUnitOfWork(options, ExecutionType.Normal, conf, mapper);
            });
            sc.AddSingleton<IHostedService>(new MaintainActivityTask(sc, conf));
            sc.AddSingleton<IHostedService>(new MaintainUsersTask(sc, conf));
            sc.AddSingleton<IJwtContext>(new JwtContext(conf, ExecutionType.Normal, new HttpClient()));

            var sp = sc.BuildServiceProvider();
            var uow = sp.GetRequiredService<IIdentityUnitOfWork<IdentityDbContext>>();

            /*
             * only live context allowed to run...
             */

            if (uow.Situation != ExecutionType.Normal)
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

            if (uow.ConfigRepo.DefaultsLegacyModeIssuer)
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
                auth.AddPolicy("AdministratorPolicy", admins =>
                {
                    admins.RequireRole("(Built-In) Administrators");
                    //admins.RequireClaim(ClaimTypes.Role, "(Built-In) Administrators");
                });
                auth.AddPolicy("UserPolicy", users =>
                {
                    users.RequireRole("(Built-In) Users");
                    //users.RequireClaim(ClaimTypes.Role, "(Built-In) Users");
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