﻿using Bhbk.Lib.Helpers.FileSystem;
using Bhbk.Lib.Helpers.Options;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Sts.Providers;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Startup
    {
        protected static FileInfo _lib = Search.DefaultPaths("appsettings-lib.json");
        protected static FileInfo _api = Search.DefaultPaths("appsettings-api.json");
        protected static IConfigurationRoot _conf;
        protected static IIdentityContext _ioc;
        protected static Microsoft.Extensions.Hosting.IHostedService[] _tasks;
        protected static IEnumerable _issuers, _audiences;

        public virtual void ConfigureContext(IServiceCollection sc)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_conf["Databases:IdentityEntities"])
                .EnableSensitiveDataLogging();

            //DRY up contexts across controllers and tasks after made thread safe...
            _ioc = new IdentityContext(options);

            sc.AddSingleton<IConfigurationRoot>(_conf);
            sc.AddSingleton<IIdentityContext>(_ioc);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainTokensTask(new IdentityContext(options)));

            var sp = sc.BuildServiceProvider();

            _conf = (IConfigurationRoot)sp.GetRequiredService<IConfigurationRoot>();
            _ioc = (IIdentityContext)sp.GetRequiredService<IIdentityContext>();
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

            _ioc.ClientMgmt.Store.Salt = _conf["IdentityClients:Salt"];
            _issuers = _conf.GetSection("IdentityClients:AllowedNames").GetChildren().Select(x => x.Value).ToList();
            _audiences = _conf.GetSection("IdentityAudiences:AllowedNames").GetChildren().Select(x => x.Value).ToList();

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
                    AudienceValidator = Bhbk.Lib.Identity.Infrastructure.AudienceValidator.MultipleAudience,
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
            sc.AddDataProtection();
            sc.AddSession(session =>
            {
                session.IdleTimeout = TimeSpan.FromSeconds(10);
                session.Cookie.HttpOnly = true;
            });
            sc.AddSwaggerGen(SwaggerOptions.ConfigureSwaggerGen);
            sc.Configure<ForwardedHeadersOptions>(headers =>
            {
                headers.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            sc.Configure<CookiePolicyOptions>(cookie =>
            {
                cookie.CheckConsentNeeded = context => true;
                cookie.MinimumSameSitePolicy = SameSiteMode.None;
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
            app.UseSession();
            app.UseMvc();

            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware

            app.UseMiddleware<AccessTokenProvider>();
            app.UseMiddleware<AuthorizationCodeProvider>();
            app.UseMiddleware<ClientCredentialsProvider>();
            app.UseMiddleware<RefreshTokenProvider>();

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAccessTokenProvider();
            //    });
            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/access", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAccessTokenProvider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/authorization", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAuthorizationCodeProvider();
            //    });
            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/authorization", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAuthorizationCodeProvider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/client", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseClientCredentialsProvider();
            //    });
            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/client", StringComparison.Ordinal)
            //   && context.Request.Method.Equals("POST")
            //   && context.Request.HasFormContentType, x =>
            //   {
            //       x.UseClientCredentialsProvider();
            //   });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/refresh", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseRefreshTokenProvider();
            //    });
            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/refresh", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseRefreshTokenProvider();
            //    });
        }
    }
}