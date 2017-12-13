using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        private static IConfigurationRoot Config;
        public static IIdentityContext Context;

        //http://asp.net-hacker.rocks/2017/05/08/add-custom-ioc-in-aspnetcore.html
        public virtual void ConfigureContext(IServiceCollection services)
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(FileHelper.SearchPaths("appsettings.json").DirectoryName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Context = new CustomIdentityContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(Config["ConnectionStrings:IdentityEntities"]));

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
            services.Configure<ForwardedHeadersOptions>(headers =>
            {
                headers.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            //order below makes big difference
            if (env.IsDevelopment())
            {
                log.AddDebug();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                log.AddConsole();
                app.UseExceptionHandler();
            }

            app.UseForwardedHeaders();
            app.UseCors(policy => policy.AllowAnyOrigin());
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}