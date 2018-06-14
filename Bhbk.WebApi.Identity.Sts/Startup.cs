using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Sts.Providers;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
        private static FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private static IConfigurationRoot _cb;
        private static IEnumerable _issuers, _audiences;

        public virtual void ConfigureContext(IServiceCollection sc)
        {
            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var ioc = new CustomIdentityContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_cb["Databases:IdentityEntities"])
                .EnableSensitiveDataLogging());

            ioc.ClientMgmt.Store.Salt = _cb["IdentityClients:Salt"];

            _issuers = _cb.GetSection("IdentityClients:AllowedNames").GetChildren().Select(x => x.Value).ToList();
            _audiences = _cb.GetSection("IdentityAudiences:AllowedNames").GetChildren().Select(x => x.Value).ToList();

            sc.AddSingleton<IIdentityContext>(ioc);
            sc.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(new MaintainTokensTask(ioc));
        }

        public virtual void ConfigureServices(IServiceCollection sc)
        {
            ConfigureContext(sc);

            var sp = sc.BuildServiceProvider();
            var ioc = sp.GetRequiredService<IIdentityContext>();

            sc.AddLogging(log => log.AddSerilog());
            sc.AddCors();
            sc.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearer =>
            {
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = ioc.ClientMgmt.Store.Get().Select(x => x.Name.ToString() + ":" + ioc.ClientMgmt.Store.Salt),
                    IssuerSigningKeys = ioc.ClientMgmt.Store.Get().Select(x => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(x.ClientKey))),
                    ValidAudiences = ioc.AudienceMgmt.Store.Get().Select(x => x.Name.ToString()),
                    AudienceValidator = CustomAudienceValidator.MultipleAudience,
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

            if (_cb == null)
                log.AddConsole();
            else
                log.AddConsole(_cb.GetSection("Logging"));

            app.UseForwardedHeaders();
            app.UseCors(policy => policy.AllowAnyOrigin());
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();

            app.UseMiddleware<AccessTokenV1Provider>();
            app.UseMiddleware<AccessTokenV2Provider>();
            app.UseMiddleware<AuthorizationCodeV1Provider>();
            app.UseMiddleware<AuthorizationCodeV2Provider>();
            app.UseMiddleware<ClientCredentialsV1Provider>();
            app.UseMiddleware<ClientCredentialsV2Provider>();
            app.UseMiddleware<RefreshTokenV1Provider>();
            app.UseMiddleware<RefreshTokenV2Provider>();

            ////https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware
            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/access", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAccessTokenV1Provider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/authorize", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAuthorizationCodeV1Provider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/client", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseClientCredentialsV1Provider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v1/refresh", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseRefreshTokenV1Provider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/access", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAccessTokenV2Provider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/authorize", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseAuthorizationCodeV2Provider();
            //    });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/client", StringComparison.Ordinal)
            //   && context.Request.Method.Equals("POST")
            //   && context.Request.HasFormContentType, x =>
            //   {
            //       x.UseClientCredentialsV2Provider();
            //   });

            //app.MapWhen(context => context.Request.Path.Equals("/oauth/v2/refresh", StringComparison.Ordinal)
            //    && context.Request.Method.Equals("POST")
            //    && context.Request.HasFormContentType, x =>
            //    {
            //        x.UseRefreshTokenV2Provider();
            //    });

        }
    }
}