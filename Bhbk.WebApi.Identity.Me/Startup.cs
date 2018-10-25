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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Me
{
    public class Startup
    {
        protected static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        protected static FileInfo _api = SearchRoots.ByAssemblyContext("appsettings-api.json");
        protected static IConfigurationRoot _conf;
        protected static IIdentityContext<AppDbContext> _uow;
        protected static IJwtContext _jwt;
        protected static Microsoft.Extensions.Hosting.IHostedService[] _tasks;

        public virtual void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_conf["Databases:IdentityEntities"])
                .EnableSensitiveDataLogging();

            //context is not thread safe yet. create new one for each background thread.
            _uow = new IdentityContext(options, ContextType.Live);
            _jwt = new JwtContext(_conf, ContextType.Live);

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IIdentityContext<AppDbContext>>(_uow);
            sc.AddSingleton<IJwtContext>(_jwt);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainQuotesTask(new IdentityContext(options, ContextType.Live)));

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _uow = (IIdentityContext<AppDbContext>)sp.GetRequiredService<IIdentityContext<AppDbContext>>();
            _jwt = (IJwtContext)sp.GetRequiredService<IJwtContext>();
            _tasks = (Microsoft.Extensions.Hosting.IHostedService[])sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
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

            _uow.ClientRepo.Salt = _conf["IdentityTenants:Salt"];

            var _issuers = (_uow.ClientRepo.GetAsync().Result)
                .Select(x => x.Name.ToString() + ":" + _uow.ClientRepo.Salt);

            //check if issuer compatibility enabled. means add issuer with no salt.
            if (_uow.ConfigRepo.DefaultsCompatibilityModeIssuer)
                _issuers = (_uow.ClientRepo.GetAsync().Result)
                    .Select(x => x.Name.ToString())
                    .Concat(_issuers);

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
                    IssuerSigningKeys = (_uow.ClientRepo.GetAsync().Result).Select(x => new SymmetricSecurityKey(Encoding.Unicode.GetBytes(x.ClientKey))).ToArray(),
                    ValidAudiences = (_uow.AudienceRepo.GetAsync().Result).Select(x => x.Name.ToString()).ToArray(),
                    AudienceValidator = Bhbk.Lib.Identity.Validators.AudienceValidator.Multiple,
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

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
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
            app.UseCors(policy => policy.AllowAnyOrigin());
            app.UseAuthentication();
            app.UseSession();
            app.UseStaticFiles();
            app.UseMvc();
            app.UseSwagger(SwaggerOptions.ConfigureSwagger);
            app.UseSwaggerUI(SwaggerOptions.ConfigureSwaggerUI);
        }
    }
}