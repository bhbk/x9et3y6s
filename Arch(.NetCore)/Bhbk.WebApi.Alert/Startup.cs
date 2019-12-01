using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Alert
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            sc.AddSingleton<IConfiguration>(conf);
            sc.AddSingleton<IContextService>(instance);
            sc.AddSingleton<IMapper>(mapper);
            sc.AddSingleton<IAuthorizationHandler, IdentityAdminsAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
            sc.AddScoped<IUoWService, UoWService>();
            sc.AddSingleton<IHostedService, QueueEmailTask>();
            sc.AddSingleton<IHostedService, QueueTextTask>();
            sc.AddSingleton<IAlertService, AlertService>();
            sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();

            /*
            * do not use dependency inject for unit of work below. is used 
            * only for owin authentication configuration.
            */

            var owin = new UoWService(conf, instance);

            if (owin.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            var allowedIssuers = conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                .Select(x => x.Value);

            var allowedAudiences = conf.GetSection("IdentityTenants:AllowedAudiences").GetChildren()
                .Select(x => x.Value);

            var issuers = owin.Issuers.Get(x => allowedIssuers.Any(y => y == x.Name))
                .Select(x => x.Name + ":" + conf["IdentityTenants:Salt"]);

            var issuerKeys = owin.Issuers.Get(x => allowedIssuers.Any(y => y == x.Name))
                .Select(x => x.IssuerKey);

            var audiences = owin.Audiences.Get(x => allowedAudiences.Any(y => y == x.Name))
                .Select(x => x.Name);

            /*
             * check if issuer compatibility enabled. means no env salt.
             */

            var legacyIssuer = owin.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyIssuer).Single();

            if (bool.Parse(legacyIssuer.ConfigValue))
                issuers = owin.Issuers.Get(x => allowedIssuers.Any(y => y == x.Name))
                    .Select(x => x.Name).Concat(issuers);

            sc.AddLogging(opt =>
            {
                opt.AddSerilog();
            });
            sc.AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
            sc.AddCors();
            sc.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {
#if RELEASE
                jwt.IncludeErrorDetails = false;
#elif !RELEASE
                jwt.IncludeErrorDetails = true;
#endif
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    AuthenticationType = "JWT:" + instance.InstanceType.ToString(),
                    ValidTypes = new List<string>() { "JWT:" + instance.InstanceType.ToString() },
                    ValidIssuers = issuers.ToArray(),
                    IssuerSigningKeys = issuerKeys.Select(x => new SymmetricSecurityKey(Encoding.Unicode.GetBytes(x))).ToArray(),
                    ValidAudiences = audiences.ToArray(),
                    AudienceValidator = AudiencesValidator.Multiple,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireAudience = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });
            sc.AddAuthorization(opt =>
            {
                opt.AddPolicy("AdministratorsPolicy", admins =>
                {
                    admins.Requirements.Add(new AlertAdminsAuthorizeRequirement());
                });
                opt.AddPolicy("ServicesPolicy", services =>
                {
                    services.Requirements.Add(new AlertServicesAuthorizeRequirement());
                });
                opt.AddPolicy("UsersPolicy", users =>
                {
                    users.Requirements.Add(new AlertUsersAuthorizeRequirement());
                });
            });
            sc.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Reference", Version = "v1" });
            });
            sc.Configure<ForwardedHeadersOptions>(opt =>
            {
                opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
        {
            //order below is important...
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseForwardedHeaders();
            app.UseStaticFiles();
            app.UseSwagger(opt =>
            {
                opt.RouteTemplate = "help/{documentName}/index.json";
            });
            app.UseSwaggerUI(opt =>
            {
                opt.RoutePrefix = "help";
                opt.SwaggerEndpoint("v1/index.json", "Reference");
            });
            app.UseRouting();
            app.UseCors(opt => opt
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(opt =>
            {
                opt.MapControllers();
            });
        }
    }
}
