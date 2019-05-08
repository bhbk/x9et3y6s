﻿using AutoMapper;
using Bhbk.Lib.Core.Options;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Services;
using Bhbk.WebApi.Identity.Sts.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            var mapper = new MapperConfiguration(x => x.AddProfile<MapperProfile>()).CreateMapper();

            sc.AddSingleton<IConfiguration>(conf);
            sc.AddSingleton<IContextService>(instance);
            sc.AddSingleton<IMapper>(mapper);
            sc.AddSingleton<IAuthorizationHandler, IdentityAdminsAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
            sc.AddScoped<IUoWService, UoWService>(x =>
            {
                return new UoWService(conf, instance);
            });
            sc.AddSingleton<IHostedService, MaintainRefreshesTask>();
            sc.AddSingleton<IHostedService, MaintainStatesTask>();
            sc.AddSingleton<IAlertService, AlertService>();

            /*
             * do not use dependency injection for unit of work below. is used 
             * only for owin authentication configuration.
             */

            var owin = new UoWService(conf, instance);

            /*
             * only live context allowed to run...
             */

            if (owin.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            var allowedIssuers = conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                .Select(x => x.Value);

            var allowedClients = conf.GetSection("IdentityTenants:AllowedClients").GetChildren()
                .Select(x => x.Value);

            var issuers = (owin.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                .Select(x => x.Name + ":" + owin.IssuerRepo.Salt);

            var issuerKeys = (owin.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                .Select(x => x.IssuerKey);

            var clients = (owin.ClientRepo.GetAsync(x => allowedClients.Any(y => y == x.Name)).Result)
                .Select(x => x.Name);

            /*
             * check if issuer compatibility enabled. means no env salt.
             */

            var legacyIssuer = (owin.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyIssuer)).Result.Single();

            if (bool.Parse(legacyIssuer.ConfigValue))
                issuers = (owin.IssuerRepo.GetAsync(x => allowedIssuers.Any(y => y == x.Name)).Result)
                    .Select(x => x.Name).Concat(issuers);
#if !RELEASE
            /*
             * add value that is hard coded just for non-production use.
             */

            issuerKeys = issuerKeys.Concat(conf.GetSection("IdentityTenants:AllowedIssuerKeys").GetChildren()
                .Select(x => x.Value));
#endif
            sc.AddLogging(log =>
            {
                log.AddSerilog();
            });
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
#if RELEASE
                bearer.IncludeErrorDetails = false;
#elif !RELEASE
                bearer.IncludeErrorDetails = true;
#endif
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = issuers.ToArray(),
                    IssuerSigningKeys = issuerKeys.Select(x => new SymmetricSecurityKey(Encoding.Unicode.GetBytes(x))).ToArray(),
                    ValidAudiences = clients.ToArray(),
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
            sc.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            sc.AddMvc().AddJsonOptions(json =>
            {
                json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            sc.AddAuthorization(auth =>
            {
                auth.AddPolicy("AdministratorsPolicy", admins =>
                {
                    admins.Requirements.Add(new IdentityAdminsAuthorizeRequirement());
                });
                auth.AddPolicy("ServicesPolicy", services =>
                {
                    services.Requirements.Add(new IdentityServicesAuthorizeRequirement());
                });
                auth.AddPolicy("UsersPolicy", users =>
                {
                    users.Requirements.Add(new IdentityUsersAuthorizeRequirement());
                });
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
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();

            //app.UseMiddleware<ResourceOwner_Deprecate>();
            //app.UseMiddleware<ResourceOwnerRefresh_Deprecate>();

            app.UseSwagger(SwaggerOptions.ConfigureSwagger);
            app.UseSwaggerUI(SwaggerOptions.ConfigureSwaggerUI);
        }
    }
}