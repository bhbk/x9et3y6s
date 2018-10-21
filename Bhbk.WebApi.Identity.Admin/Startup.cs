﻿using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        protected static FileInfo _lib = Search.DefaultPaths("appsettings-lib.json");
        protected static FileInfo _api = Search.DefaultPaths("appsettings-api.json");
        protected static IConfigurationRoot _conf;
        protected static IIdentityContext _ioc;
        protected static Microsoft.Extensions.Hosting.IHostedService[] _tasks;
        protected static IS2SJwtContext _jwt;

        public virtual void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_conf["Databases:IdentityEntities"])
                .EnableSensitiveDataLogging();

            //DRY up contexts across controllers and tasks after made thread safe...
            _ioc = new IdentityContext(options);
            _jwt = new S2SJwtContext(_conf, _ioc.ContextStatus);

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IIdentityContext>(_ioc);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainActivityTask(new IdentityContext(options)));
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainUsersTask(new IdentityContext(options)));
            sc.AddSingleton<IS2SJwtContext>(_jwt);

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _ioc = (IIdentityContext)sp.GetRequiredService<IIdentityContext>();
            _tasks = (Microsoft.Extensions.Hosting.IHostedService[])sp.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            _jwt = (IS2SJwtContext)sp.GetRequiredService<IS2SJwtContext>();
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

            _ioc.ClientMgmt.Store.Salt = _conf["IdentityClients:Salt"];

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
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = _ioc.ClientMgmt.Store.Get().Select(x => x.Name.ToString() + ":" + _ioc.ClientMgmt.Store.Salt),
                    IssuerSigningKeys = _ioc.ClientMgmt.Store.Get().Select(x => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(x.ClientKey))),
                    ValidAudiences = _ioc.AudienceMgmt.Store.Get().Select(x => x.Name.ToString()),
                    AudienceValidator = Bhbk.Lib.Core.Validators.IdentityAudience.Multiple,
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
            sc.AddMvc().AddMvcOptions(binder =>
            {
                binder.UseMyModelBinderProvider();
            });
            sc.AddMvc().AddControllersAsServices();
            sc.AddMvc().AddJsonOptions(json =>
            {
                json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            //sc.AddSession();
            sc.AddSwaggerGen(SwaggerOptions.ConfigureSwaggerGen);
            sc.Configure<ForwardedHeadersOptions>(headers =>
            {
                headers.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            //order below is important...
            if (env.IsDevelopment())
            {
                log.AddDebug();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            if (_conf == null)
                log.AddConsole();
            else
                log.AddConsole(_conf.GetSection("Logging"));

            app.UseForwardedHeaders();
            app.UseCors(policy => policy.AllowAnyOrigin());
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseSwagger(SwaggerOptions.ConfigureSwagger);
            app.UseSwaggerUI(SwaggerOptions.ConfigureSwaggerUI);
            //app.UseSession();
            app.UseMvc();
        }
    }
}