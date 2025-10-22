using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Data.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Identity.Data.EF.Infrastructure
{
    public class UnitOfWork : GenericUnitOfWork<IdentityEntities>, IUnitOfWork
    {
        private readonly ILoggerFactory _logger;
        public AudienceRepository Audiences { get; private set; }
        public IGenericRepository<tbl_UserAuthActivity> UserAuthActivities { get; private set; }
        public IGenericRepository<tbl_AudienceAuthActivity> AudienceAuthActivities { get; private set; }
        public IGenericRepository<tbl_ChatConversation> ChatConversations { get; private set; }
        public IGenericRepository<tbl_ChatFavorite> ChatFavorites { get; private set; }
        public IGenericRepository<tbl_ChatFile> ChatFiles { get; private set; }
        public IGenericRepository<tbl_ChatMessage> ChatMessages { get; private set; }
        public IGenericRepository<tbl_ChatPrompt> ChatPrompts { get; private set; }
        public IGenericRepository<tbl_ChatPromptHistory> ChatPromptHistories { get; private set; }
        public IGenericRepository<tbl_Claim> Claims { get; private set; }
        public IGenericRepository<tbl_EmailActivity> EmailActivity { get; private set; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; private set; }
        public IGenericRepository<tbl_UserEntitlement> UserEntitlements { get; private set; }
        public IGenericRepository<tbl_AudienceEntitlement> AudienceEntitlements { get; private set; }
        public IGenericRepository<tbl_EntitlementScope> EntitlementScopes { get; private set; }
        public IGenericRepository<tbl_EntitlementType> EntitlementTypes { get; private set; }
        public IGenericRepository<tbl_Issuer> Issuers { get; private set; }
        public IGenericRepository<tbl_Job> Jobs { get; private set; }
        public IGenericRepository<tbl_JobSetting> JobSettings { get; private set; }
        public IGenericRepository<tbl_LLMProvider> LLMProviders { get; private set; }
        public IGenericRepository<tbl_LLMProviderSetting> LLMProviderSettings { get; private set; }
        public IGenericRepository<tbl_LoginProvider> LoginProviders { get; private set; }
        public IGenericRepository<tbl_Quote> Quotes { get; private set; }
        public IGenericRepository<tbl_Refresh> Refreshes { get; private set; }
        public IGenericRepository<tbl_Role> Roles { get; private set; }
        public IGenericRepository<tbl_Setting> Settings { get; private set; }
        public IGenericRepository<tbl_State> States { get; private set; }
        public IGenericRepository<tbl_TextActivity> TextActivity { get; private set; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; private set; }
        public IGenericRepository<tbl_Url> Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService env)
            : base(CreateContext(connection, env), env.InstanceType)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning);
            });

            InitializeRepositories(env);
        }

        private static IdentityEntities CreateContext(string connection, IContextService env)
        {
            switch (env.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection)
                            .EnableSensitiveDataLogging();
#else
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection);
#endif
                        var context = new IdentityEntities(builder.Options);
                        context.ChangeTracker.LazyLoadingEnabled = false;
                        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
                        return context;
                    }

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:")
                            .EnableSensitiveDataLogging();
#else
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:");
#endif
                        var context = new IdentityEntities(builder.Options);
                        context.ChangeTracker.LazyLoadingEnabled = false;
                        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
                        return context;
                    }

                case InstanceContext.UnitTest:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        private void InitializeRepositories(IContextService env)
        {
            var context = (IdentityEntities)DbContext;

            Audiences = new AudienceRepository(context, env);
            UserAuthActivities = new GenericRepository<tbl_UserAuthActivity>(context);
            AudienceAuthActivities = new GenericRepository<tbl_AudienceAuthActivity>(context);
            ChatConversations = new GenericRepository<tbl_ChatConversation>(context);
            ChatFavorites = new GenericRepository<tbl_ChatFavorite>(context);
            ChatFiles = new GenericRepository<tbl_ChatFile>(context);
            ChatMessages = new GenericRepository<tbl_ChatMessage>(context);
            ChatPrompts = new GenericRepository<tbl_ChatPrompt>(context);
            ChatPromptHistories = new GenericRepository<tbl_ChatPromptHistory>(context);
            Claims = new GenericRepository<tbl_Claim>(context);
            EmailQueue = new GenericRepository<tbl_EmailQueue>(context);
            EmailActivity = new GenericRepository<tbl_EmailActivity>(context);
            UserEntitlements = new GenericRepository<tbl_UserEntitlement>(context);
            AudienceEntitlements = new GenericRepository<tbl_AudienceEntitlement>(context);
            EntitlementScopes = new GenericRepository<tbl_EntitlementScope>(context);
            EntitlementTypes = new GenericRepository<tbl_EntitlementType>(context);
            Issuers = new GenericRepository<tbl_Issuer>(context);
            Jobs = new GenericRepository<tbl_Job>(context);
            JobSettings = new GenericRepository<tbl_JobSetting>(context);
            LLMProviders = new GenericRepository<tbl_LLMProvider>(context);
            LLMProviderSettings = new GenericRepository<tbl_LLMProviderSetting>(context);
            LoginProviders = new GenericRepository<tbl_LoginProvider>(context);
            Quotes = new GenericRepository<tbl_Quote>(context);
            Refreshes = new GenericRepository<tbl_Refresh>(context);
            Roles = new GenericRepository<tbl_Role>(context);
            Settings = new GenericRepository<tbl_Setting>(context);
            States = new GenericRepository<tbl_State>(context);
            TextQueue = new GenericRepository<tbl_TextQueue>(context);
            TextActivity = new GenericRepository<tbl_TextActivity>(context);
            Urls = new GenericRepository<tbl_Url>(context);
            Users = new UserRepository(context, env);
        }

        public new void Dispose()
        {
            _logger?.Dispose();
            base.Dispose();
        }
    }
}
