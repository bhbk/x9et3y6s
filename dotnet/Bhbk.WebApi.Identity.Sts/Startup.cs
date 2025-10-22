using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Identity.Sts.Jobs;
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var workerName = "IdentityStsWorker";

            var encryptionKey = Environment.GetEnvironmentVariable("CONFIG_ENCRYPTION_KEY");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddEncryptedJsonFile("appsettings.json", encryptionKey, optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            var map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>())
                .CreateMapper();

            sc.AddSingleton<IConfiguration>(conf);
            sc.AddSingleton<IContextService>(env);
            sc.AddSingleton<IMapper>(map);
            sc.AddSingleton<IAuthorizationHandler, IdentityUsersAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
            sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
            {
                return new UnitOfWork(conf["Databases:IdentityEntities_EF"], env);
            });
            sc.AddSingleton<IAlertService, AlertService>(_ =>
            {
                return new AlertService
                {
                    Grant = new ClientCredentialGrantV2()
                };
            });
            sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();

            var jobSettings = LoadJobSettings(conf["Databases:IdentityEntities_EF"]);

            sc.AddQuartz(jobs =>
            {
                jobs.SchedulerId = Guid.NewGuid().ToString();

                jobs.UseMicrosoftDependencyInjectionJobFactory();
                jobs.UseSimpleTypeLoader();
                jobs.UseInMemoryStore();
                jobs.UseDefaultThreadPool();

                /* https://www.freeformatter.com/cron-expression-generator-quartz.html */

                if (jobSettings.TryGetValue("Maintain Refreshes", out var maintainRefreshes) && maintainRefreshes.IsEnabled)
                {
                    var jobKey = new JobKey(typeof(MaintainRefreshesJob).Name, workerName);
                    jobs.AddJob<MaintainRefreshesJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in maintainRefreshes.Schedules)
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(jobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );

                        Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                    }
                }

                if (jobSettings.TryGetValue("Maintain States", out var maintainStates) && maintainStates.IsEnabled)
                {
                    var jobKey = new JobKey(typeof(MaintainStatesJob).Name, workerName);
                    jobs.AddJob<MaintainStatesJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in maintainStates.Schedules)
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

            if (env.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            /*
             * do not use dependency injection for unit of work below. is used 
             * only for owin authentication configuration.
             */

            var seeds = new UnitOfWork(conf["Databases:IdentityEntities_EF"], env);

            var issuers = conf.GetSection("IdentityTenant:AllowedIssuers").GetChildren()
                .Select(x => x.Value + ":" + conf["IdentityTenant:Salt"]);

            var issuerKeys = conf.GetSection("IdentityTenant:AllowedIssuerKeys").GetChildren()
                .Select(x => x.Value);

            var audiences = conf.GetSection("IdentityTenant:AllowedAudiences").GetChildren()
                .Select(x => x.Value);

            /*
             * check if issuer compatibility enabled. means no env salt.
             */

            var legacyIssuer = seeds.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyIssuer).Single();

            if (bool.Parse(legacyIssuer.ConfigValue))
                issuers = conf.GetSection("IdentityTenant:AllowedIssuers").GetChildren()
                .Select(x => x.Value).Concat(issuers);

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
                opt.AddPolicy(PolicyConstants.OAuth2ROPGrants, humans =>
                {
                    humans.Requirements.Add(new IdentityUsersAuthorizeRequirement());
                });
                opt.AddPolicy(PolicyConstants.OAuth2CCGrants, servers =>
                {
                    servers.Requirements.Add(new IdentityServicesAuthorizeRequirement());
                });

                /* entitlement-based policies (database-driven RBAC) */
                opt.AddPolicy(PolicyConstants.EntitlementAdminPolicy, policy =>
                    policy.Requirements.Add(new IdentityEntitlementRequirement("Admin")));
                opt.AddPolicy(PolicyConstants.EntitlementUserPolicy, policy =>
                    policy.Requirements.Add(new IdentityEntitlementRequirement("User")));
                opt.AddPolicy(PolicyConstants.EntitlementViewerPolicy, policy =>
                    policy.Requirements.Add(new IdentityEntitlementRequirement("Viewer")));
            });
            sc.AddScoped<IAuthorizationHandler, IdentityEntitlementAuthorize>();
            sc.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Reference", Version = "v1" });
            });
            sc.Configure<ForwardedHeadersOptions>(opt =>
            {
                opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log, IConfiguration conf)
        {
            /* order below is important */
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
            var allowedOrigins = conf.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            app.UseCors(opt => opt
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(opt =>
            {
                opt.MapControllers();
            });
        }

        private static Dictionary<string, JobSettingsEntry> LoadJobSettings(string connectionString)
        {
            using var uow = new UnitOfWork(connectionString);

            var result = new Dictionary<string, JobSettingsEntry>();

            foreach (var job in uow.Jobs.Get().ToList())
            {
                var settings = uow.JobSettings.Get(s => s.JobId == job.Id).ToList();

                result[job.Name] = new JobSettingsEntry
                {
                    IsEnabled = job.IsEnabled,
                    Schedules = settings.Where(s => s.ConfigKey == "Schedule").Select(s => s.ConfigValue).ToList(),
                    Settings = settings.Where(s => s.ConfigKey != "Schedule").ToDictionary(s => s.ConfigKey, s => s.ConfigValue),
                };
            }

            return result;
        }
    }

    internal class JobSettingsEntry
    {
        public bool IsEnabled { get; set; }
        public List<string> Schedules { get; set; } = new();
        public Dictionary<string, string> Settings { get; set; } = new();
    }
}