using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Core.Providers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        protected static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        protected static FileInfo _api = SearchRoots.ByAssemblyContext("appsettings-api.json");
        private IEnumerable<string> _issuers, _issuerKeys, _clients;

        public virtual void ConfigureContext(IServiceCollection sc)
        {
            var conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conf["Databases:IdentityEntities"])
                .EnableSensitiveDataLogging();

            /*
             * https://dotnetcoretutorials.com/2017/03/25/net-core-dependency-injection-lifetimes-explained/
             * transient: persist for work. lot of overhead and stateless.
             * scoped: persist for request. default for framework middlewares/controllers.
             * singleton: persist for application execution. thread safe needed for uow/ef context.
             * 
             * order below matters...
             */

            sc.AddSingleton<IConfigurationRoot>(conf);
            sc.AddSingleton<IJwtContext>(new JwtContext(conf, ContextType.Live));
            sc.AddScoped<IIdentityContext<AppDbContext>>(x =>
            {
                return new IdentityContext(options, ContextType.Live);
            });
            sc.AddSingleton<IHostedService>(new MaintainActivityTask(sc));
            sc.AddSingleton<IHostedService>(new MaintainUsersTask(sc));
        }

        public virtual void ConfigureServices(IServiceCollection sc)
        {
            ConfigureContext(sc);

            var sp = sc.BuildServiceProvider();

            var conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            var uow = (IIdentityContext<AppDbContext>)sp.GetRequiredService<IIdentityContext<AppDbContext>>();
            var tasks = (IHostedService[])sp.GetServices<IHostedService>();

            if (uow.Situation == ContextType.Live)
            {
                var allowedIssuers = conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                    .Select(x => x.Value);

                var allowedClients = conf.GetSection("IdentityTenants:AllowedClients").GetChildren()
                    .Select(x => x.Value);

                _issuers = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name + ":" + uow.IssuerRepo.Salt);

                _issuerKeys = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.IssuerKey);

                _clients = (uow.ClientRepo.GetAsync(x => allowedClients.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name);

                //check if issuer compatibility enabled. means no env salt.
                if (uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                    _issuers = (uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                        .Select(x => x.Name).Concat(_issuers);

#if DEBUG
                //check if in debug. add value that is hard coded just for that use.
                _issuerKeys = _issuerKeys.Concat(new[]
                {
                    conf.GetSection("IdentityTenants:AllowedIssuerKeys").GetChildren().First().Value
                });
#endif
            }
            else if (uow.Situation == ContextType.UnitTest)
            {
                _issuers = (uow.IssuerRepo.GetAsync().Result)
                    .Select(x => x.Name + ":" + uow.IssuerRepo.Salt);

                _issuerKeys = (uow.IssuerRepo.GetAsync().Result)
                    .Select(x => x.IssuerKey);

                _clients = (uow.ClientRepo.GetAsync().Result)
                    .Select(x => x.Name);

                //check if issuer compatibility enabled. means no env salt.
                if (uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                    _issuers = (uow.IssuerRepo.GetAsync().Result)
                        .Select(x => x.Name).Concat(_issuers);
            }
            else
                throw new NotSupportedException();

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
                    ValidIssuers = _issuers.ToArray(),
                    IssuerSigningKeys = _issuerKeys.Select(x => new SymmetricSecurityKey(Encoding.Unicode.GetBytes(x))).ToArray(),
                    ValidAudiences = _clients.ToArray(),
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
            sc.AddSession();
            sc.AddMvc();
            sc.AddMvc().AddMvcOptions(binder =>
            {
                binder.UseBhbkPagingBinderProvider();
            });
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