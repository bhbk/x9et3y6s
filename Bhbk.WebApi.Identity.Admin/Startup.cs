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
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        private static FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");
        private static IConfigurationRoot _cb;
        private static IEnumerable _clients, _audiences;

        //http://asp.net-hacker.rocks/2017/05/08/add-custom-ioc-in-aspnetcore.html
        public virtual void ConfigureContext(IServiceCollection sc)
        {
            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _clients = _cb.GetSection("Client:Names").GetChildren().Select(x => x.Value).ToList();
            _audiences = _cb.GetSection("Audience:Names").GetChildren().Select(x => x.Value).ToList();

            var ioc = new CustomIdentityContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_cb["ConnectionStrings:IdentityEntities"]));

            ioc.ClientMgmt.Store.Salt = _cb["Client:Salt"];

            sc.AddSingleton<IIdentityContext>(ioc);
            sc.AddSingleton<IHostedService>(new MaintainUsersTask(ioc));
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
                    ValidIssuers = ioc.ClientMgmt.Store.Get().Select(x => x.Id.ToString() + ":" + ioc.ClientMgmt.Store.Salt),
                    IssuerSigningKeys = ioc.ClientMgmt.Store.Get().Select(x => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(x.ClientKey))),
                    ValidAudiences = ioc.AudienceMgmt.Store.Get().Select(x => x.Id.ToString()),
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