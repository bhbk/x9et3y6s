using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
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

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conf["Databases:IdentityEntities"]);

            var mapper = new MapperConfiguration(x =>
            {
                x.AddProfile<IdentityMappings>();
                x.AddExpressionMapping();
            }).CreateMapper();

            sc.AddSingleton(mapper);
            sc.AddSingleton(conf);
            sc.AddScoped<IIdentityContext<AppDbContext>>(x =>
            {
                return new IdentityContext(options, ContextType.Live, conf, mapper);
            });
            sc.AddSingleton<IHostedService>(new MaintainActivityTask(sc, conf));
            sc.AddSingleton<IHostedService>(new MaintainUsersTask(sc, conf));
            sc.AddSingleton<IJwtContext>(new JwtContext(conf, ContextType.Live, new HttpClient()));

            var sp = sc.BuildServiceProvider();
            var uow = sp.GetRequiredService<IIdentityContext<AppDbContext>>();

            /*
             * only live context allowed to run...
             */

            if (uow.Situation != ContextType.Live)
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

            if (uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                issuers = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name).Concat(issuers);

#if DEBUG
            /*
             * check if in debug. add value that is hard coded just for that use.
             */

            issuerKeys = issuerKeys.Concat(conf.GetSection("IdentityTenants:AllowedIssuerKeys").GetChildren()
                .Select(x => x.Value));
#endif

            sc.AddLogging(log => log.AddSerilog());
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
            sc.AddAuthorization(auth =>
                auth.AddPolicy("AdministratorPolicy", policy =>
                {
                    policy.RequireRole("Bhbk.WebApi.Identity(Admins)");
                }));
            sc.AddSession();
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
            app.UseAuthentication();
            app.UseSession();
            app.UseStaticFiles();
            app.UseMvc();
            app.UseSwagger(SwaggerOptions.ConfigureSwagger);
            app.UseSwaggerUI(SwaggerOptions.ConfigureSwaggerUI);
        }
    }
}