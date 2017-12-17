using Bhbk.Lib.Identity.Helpers;
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
using Serilog;
using System;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        private static IConfigurationRoot _config;
        private static IIdentityContext _ioc;

        //http://asp.net-hacker.rocks/2017/05/08/add-custom-ioc-in-aspnetcore.html
        public virtual void ConfigureContext(IServiceCollection sc)
        {
            var location = FileSystemHelper.SearchUsualPaths("appsettings.json");

            _config = new ConfigurationBuilder()
                .SetBasePath(location.DirectoryName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _ioc = new CustomIdentityContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_config["ConnectionStrings:IdentityEntities"]));

            sc.AddSingleton<IIdentityContext>(_ioc);
            sc.AddSingleton<IHostedService>(new MaintainUsersTask(_ioc));
            sc.AddSingleton<IHostedService>(new TestingTask(_ioc));
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
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeys = ioc.AudienceMgmt.Store.GetAll().Select(x => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(x.AudienceKey))),
                    ValidIssuers = ioc.ClientMgmt.Store.GetAll().Select(x => x.Id.ToString()),
                    ValidAudiences = ioc.AudienceMgmt.Store.GetAll().Select(x => x.Id.ToString()),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
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
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
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