﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Alert.Jobs;
using Bhbk.WebApi.Alert.Services;
using CronExpressionDescriptor;
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
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bhbk.WebApi.Alert
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_DIRECT>()).CreateMapper();

            sc.AddSingleton<IConfiguration>(conf);
            sc.AddSingleton<IContextService>(instance);
            sc.AddSingleton<IMapper>(mapper);
            sc.AddSingleton<IAuthorizationHandler, IdentityUserssAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
            sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
            {
                return new UnitOfWork(conf["Databases:IdentityEntities"], instance);
            });
            sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();
            sc.AddSingleton<IAlertService, AlertService>(_ =>
            {
                var alert = new AlertService(conf);
                alert.Grant = new ClientCredentialGrantV2(conf);

                return alert;
            });
            sc.AddScoped<ITwilioService, TwilioService>();
            sc.AddScoped<ISendgridService, SendgridService>();
            sc.AddQuartz(jobs =>
            {
                jobs.SchedulerId = Guid.NewGuid().ToString();

                jobs.UseMicrosoftDependencyInjectionJobFactory();
                jobs.UseSimpleTypeLoader();
                jobs.UseInMemoryStore();
                jobs.UseDefaultThreadPool();

                //https://www.freeformatter.com/cron-expression-generator-quartz.html

                if (bool.Parse(conf["Jobs:CleanupEmails:Enable"]))
                {
                    var jobKey = new JobKey(JobType.CleanupEmailsJob.ToString(), WorkerType.AlertWorker.ToString());
                    jobs.AddJob<CleanupEmailsJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:CleanupEmails:Schedules").GetChildren()
                        .Select(x => x.Value).ToList())
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(jobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );

                        Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                    }
                }

                if (bool.Parse(conf["Jobs:CleanupTexts:Enable"]))
                {
                    var jobKey = new JobKey(JobType.CleanupTextsJob.ToString(), WorkerType.AlertWorker.ToString());
                    jobs.AddJob<CleanupTextsJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:CleanupTexts:Schedules").GetChildren()
                        .Select(x => x.Value).ToList())
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(jobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );

                        Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                    }
                }

                if (bool.Parse(conf["Jobs:DequeueEmails:Enable"]))
                {
                    var jobKey = new JobKey(JobType.DequeueEmailsJob.ToString(), WorkerType.AlertWorker.ToString());
                    jobs.AddJob<DequeueEmailsJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:DequeueEmails:Schedules").GetChildren()
                        .Select(x => x.Value).ToList())
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(jobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );

                        Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                    }
                }

                if (bool.Parse(conf["Jobs:DequeueTexts:Enable"]))
                {
                    var jobKey = new JobKey(JobType.DequeueTextsJob.ToString(), WorkerType.AlertWorker.ToString());
                    jobs.AddJob<DequeueTextsJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:DequeueTexts:Schedules").GetChildren()
                        .Select(x => x.Value).ToList())
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(jobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );

                        Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                    }
                }
            });
            sc.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            if (instance.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            /*
            * do not use dependency inject for unit of work below. is used 
            * only for owin authentication configuration.
            */

            var seeds = new UnitOfWork(conf["Databases:IdentityEntities"], instance);

            var allowedIssuers = conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                .Select(x => x.Value);

            var allowedAudiences = conf.GetSection("IdentityTenants:AllowedAudiences").GetChildren()
                .Select(x => x.Value);

            var issuers = seeds.Issuers.Get(x => allowedIssuers.Any(y => y == x.Name))
                .Select(x => x.Name + ":" + conf["IdentityTenants:Salt"]);

            var issuerKeys = seeds.Issuers.Get(x => allowedIssuers.Any(y => y == x.Name))
                .Select(x => x.IssuerKey);

            var audiences = seeds.Audiences.Get(x => allowedAudiences.Any(y => y == x.Name))
                .Select(x => x.Name);

            /*
             * check if issuer compatibility enabled. means no env salt.
             */

            var legacyIssuer = seeds.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

            if (bool.Parse(legacyIssuer.ConfigValue))
                issuers = seeds.Issuers.Get(x => allowedIssuers.Any(y => y == x.Name))
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
                };
            });
            sc.AddAuthorization(opt =>
            {
                opt.AddPolicy(Constants.DefaultPolicyForHumans, humans =>
                {
                    humans.Requirements.Add(new IdentityUsersAuthorizeRequirement());
                });
                opt.AddPolicy(Constants.DefaultPolicyForServices, servers =>
                {
                    servers.Requirements.Add(new IdentityServicesAuthorizeRequirement());
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
