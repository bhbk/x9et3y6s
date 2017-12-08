﻿using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Sts.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Startup
    {
        public static IIdentityContext Context;

        public virtual void ConfigureContext(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(FileHelper.FindFileInDefaultPaths("appsettings.json").DirectoryName)
                .AddJsonFile("appsettings.json")
                .Build();

            Context = new CustomIdentityContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(config["ConnectionStrings:IdentityEntities"]));

            services.AddSingleton<IIdentityContext>(Context);
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureContext(services);

            var sp = services.BuildServiceProvider();
            var ioc = sp.GetService<IIdentityContext>();

            services.AddLogging();
            services.AddCors();
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearer =>
            {
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeys = ioc.AudienceMgmt.Store.GetAll().Select(x => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(x.AudienceKey))),
                    ValidIssuers = ioc.ClientMgmt.Store.GetAll().Select(x => x.Id.ToString()),
                    ValidAudiences = ioc.AudienceMgmt.Store.GetAll().Select(x => x.Id.ToString()),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                };
            });
            services.AddMvc();
            services.AddMvc().AddControllersAsServices();
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            //order below makes big difference
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseCors(policy => policy.AllowAnyOrigin());
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

            //explicitely mapped elements for STS. want to avoid chained middleware to hit these end-points.
            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware
            app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseAccessTokenProviderV1();
                });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/authorize", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseAuthorizationCodeProviderV1();
                });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/client", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseClientCredentialsProviderV1();
                });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/refresh", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseRefreshTokenProviderV1();
                });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/access", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseAccessTokenProviderV2();
                });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/authorize", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseAuthorizationCodeProviderV2();
                });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/client", StringComparison.Ordinal)
               && context.Request.Method.Equals("POST")
               && context.Request.HasFormContentType, x =>
               {
                   x.UseClientCredentialsProviderV2();
               });

            app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/refresh", StringComparison.Ordinal)
                && context.Request.Method.Equals("POST")
                && context.Request.HasFormContentType, x =>
                {
                    x.UseRefreshTokenProviderV2();
                });

        }
    }
}