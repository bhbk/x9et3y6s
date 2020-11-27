using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure_TBL;
using Bhbk.Lib.Identity.Domain.Authorize;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Identity.Admin.Jobs;
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

namespace Bhbk.WebApi.Identity.Admin
{
    public class Startup
    {
        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var workerName = "AdminWorker";

            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TBL>()).CreateMapper();

            sc.AddSingleton<IConfiguration>(conf);
            sc.AddSingleton<IContextService>(instance);
            sc.AddSingleton<IMapper>(mapper);
            sc.AddSingleton<IAuthorizationHandler, IdentityUserssAuthorize>();
            sc.AddSingleton<IAuthorizationHandler, IdentityServicesAuthorize>();
            sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
            {
                return new UnitOfWork(conf["Databases:IdentityEntities"], instance);
            });
            sc.AddSingleton<IAlertService, AlertService>(_ =>
            {
                return new AlertService
                {
                    Grant = new ClientCredentialGrantV2()
                };
            });
            sc.AddSingleton<IOAuth2JwtFactory, OAuth2JwtFactory>();
            sc.AddQuartz(jobs =>
            {
                jobs.SchedulerId = Guid.NewGuid().ToString();

                jobs.UseMicrosoftDependencyInjectionJobFactory();
                jobs.UseSimpleTypeLoader();
                jobs.UseInMemoryStore();
                jobs.UseDefaultThreadPool();

                //https://www.freeformatter.com/cron-expression-generator-quartz.html

                if (bool.Parse(conf["Jobs:MaintainActivity:Enable"]))
                {
                    var jobKey = new JobKey(typeof(MaintainActivityJob).Name, workerName);
                    jobs.AddJob<MaintainActivityJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:MaintainActivity:Schedules").GetChildren()
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

                if (bool.Parse(conf["Jobs:MaintainUsers:Enable"]))
                {
                    var jobKey = new JobKey(typeof(MaintainUsersJob).Name, workerName);
                    jobs.AddJob<MaintainUsersJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:MaintainUsers:Schedules").GetChildren()
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
             * do not use dependency injection for unit of work below. is used 
             * only for owin authentication configuration.
             */

            var seeds = new UnitOfWork(conf["Databases:IdentityEntities"], instance);

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
                && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

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
                    //AuthenticationType = "JWT:" + instance.InstanceType.ToString(),
                    //ValidTypes = new List<string>() { "JWT:" + instance.InstanceType.ToString() },
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