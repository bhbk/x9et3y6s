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
using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Configuration;
using Bhbk.Lib.Identity.LLM.Providers;
using Bhbk.Lib.Identity.Validators;
using Bhbk.WebApi.Identity.User.Hubs;
using Bhbk.WebApi.Identity.User.Jobs;
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
using Microsoft.Extensions.Options;
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
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.User
{
    public class Startup
    {
        private IConfiguration conf;

        public virtual void ConfigureServices(IServiceCollection sc)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var workerName = "IdentityMeWorker";

            var encryptionKey = Environment.GetEnvironmentVariable("CONFIG_ENCRYPTION_KEY");

            conf = (IConfiguration)new ConfigurationBuilder()
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

            var llmSettings = LoadLLMProviderSettings(conf["Databases:IdentityEntities_EF"]);
            sc.AddSingleton(Microsoft.Extensions.Options.Options.Create(llmSettings));

            if (llmSettings.Failover.Count > 0)
            {
                sc.AddSingleton<ILLMProvider>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<LLMProviderSettings>>();
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var providers = new List<ILLMProvider>();

                    foreach (var name in llmSettings.Failover)
                    {
                        ILLMProvider provider = name switch
                        {
                            "AWSBedrock" when options.Value.AWSBedrock.Enabled =>
                                new AWSBedrockLLMProvider(options),
                            "Ollama" when options.Value.Ollama.Enabled =>
                                new OllamaLLMProvider(options, loggerFactory.CreateLogger<OllamaLLMProvider>()),
                            "AzureOpenAI" when options.Value.AzureOpenAI.Enabled =>
                                new AzureOpenAILLMProvider(options),
                            "GoogleVertexAI" when options.Value.VertexAI.Enabled =>
                                new GoogleVertexAILLMProvider(options),
                            _ => null
                        };
                        if (provider != null) providers.Add(provider);
                    }

                    if (providers.Count == 0)
                        throw new InvalidOperationException(
                            "LLM providers are configured in the database but none are enabled");
                    if (providers.Count == 1)
                        return providers[0];

                    return new FailoverLLMProvider(providers, loggerFactory.CreateLogger<FailoverLLMProvider>());
                });
            }

            sc.AddSignalR();

            var jobSettings = LoadJobSettings(conf["Databases:IdentityEntities_EF"]);

            sc.AddQuartz(jobs =>
            {
                jobs.SchedulerId = Guid.NewGuid().ToString();

                jobs.UseMicrosoftDependencyInjectionJobFactory();
                jobs.UseSimpleTypeLoader();
                jobs.UseInMemoryStore();
                jobs.UseDefaultThreadPool();

                /* https://www.freeformatter.com/cron-expression-generator-quartz.html */

                if (jobSettings.TryGetValue("Maintain Quotes", out var maintainQuotes) && maintainQuotes.IsEnabled)
                {
                    var jobKey = new JobKey(typeof(MaintainQuotesJob).Name, workerName);
                    jobs.AddJob<MaintainQuotesJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in maintainQuotes.Schedules)
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

                /* SignalR sends token as query param during WebSocket handshake */
                jwt.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
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

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
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
                opt.MapHub<ChatHub>("/hubs/chat");
            });
        }

        private static LLMProviderSettings LoadLLMProviderSettings(string connectionString)
        {
            using var uow = new UnitOfWork(connectionString);

            var dbProviders = uow.LLMProviders.Get()
                .OrderBy(p => p.FailoverOrder)
                .ToList();

            var configs = new List<(string Name, bool Enabled, int FailoverOrder, IDictionary<string, string> Settings)>();

            foreach (var provider in dbProviders)
            {
                var settings = uow.LLMProviderSettings.Get(s => s.ProviderId == provider.Id)
                    .ToDictionary(s => s.ConfigKey, s => s.ConfigValue);

                configs.Add((provider.Name, provider.IsEnabled, provider.FailoverOrder, settings));
            }

            return LLMProviderSettings.FromProviderConfigs(configs);
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