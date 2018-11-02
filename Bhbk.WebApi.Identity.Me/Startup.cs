using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Me.Tasks;
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

namespace Bhbk.WebApi.Identity.Me
{
    public class Startup
    {
        protected static IConfigurationRoot _conf;
        protected static IJwtContext _jwt;
        protected static IHostedService[] _tasks;
        protected static IIdentityContext<AppDbContext> _uow;
        protected static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        protected static FileInfo _api = SearchRoots.ByAssemblyContext("appsettings-api.json");
        private IEnumerable<string> _issuers, _issuerKeys, _clients;

        public virtual void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_conf["Databases:IdentityEntities"])
                .EnableSensitiveDataLogging();

            /*
             * https://dotnetcoretutorials.com/2017/03/25/net-core-dependency-injection-lifetimes-explained/
             * transient: persist for work. lot of overhead and stateless.
             * scoped: persist for request. default for framework middlewares/controllers.
             * singleton: persist for application execution. thread safe needed for uow/ef context.
             */

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IJwtContext>(new JwtContext(_conf, ContextType.Live));
            sc.AddSingleton<IHostedService>(new MaintainQuotesTask(new IdentityContext(options, ContextType.Live)));
            sc.AddScoped<IIdentityContext<AppDbContext>>(x =>
            {
                return new IdentityContext(options, ContextType.Live);
            });

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _jwt = (IJwtContext)sp.GetRequiredService<IJwtContext>();
            _tasks = (IHostedService[])sp.GetServices<IHostedService>();
            _uow = (IIdentityContext<AppDbContext>)sp.GetRequiredService<IIdentityContext<AppDbContext>>();
        }

        public virtual void ConfigureServices(IServiceCollection sc)
        {
            _conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            ConfigureContext(sc);

            if (_uow.Situation == ContextType.Live)
            {
                var allowedIssuers = _conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                    .Select(x => x.Value);

                var allowedClients = _conf.GetSection("IdentityTenants:AllowedClients").GetChildren()
                    .Select(x => x.Value);

                _issuers = (_uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name + ":" + _uow.IssuerRepo.Salt);

                _issuerKeys = (_uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.IssuerKey);

                _clients = (_uow.ClientRepo.GetAsync(x => allowedClients.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name);

                //check if issuer compatibility enabled. means no env salt.
                if (_uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                    _issuers = (_uow.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                        .Select(x => x.Name).Concat(_issuers);

#if DEBUG
                //check if in debug. add value that is hard coded just for that use.
                _issuerKeys = _issuerKeys.Concat(new[]
                {
                    _conf.GetSection("IdentityTenants:AllowedIssuerKeys").GetChildren().First().Value
                });
#endif
            }
            else if (_uow.Situation == ContextType.UnitTest)
            {
                _issuers = (_uow.IssuerRepo.GetAsync().Result)
                    .Select(x => x.Name + ":" + _uow.IssuerRepo.Salt);

                _issuerKeys = (_uow.IssuerRepo.GetAsync().Result)
                    .Select(x => x.IssuerKey);

                _clients = (_uow.ClientRepo.GetAsync().Result)
                    .Select(x => x.Name);

                //check if issuer compatibility enabled. means no env salt.
                if (_uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                    _issuers = (_uow.IssuerRepo.GetAsync().Result)
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