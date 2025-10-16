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
    public class UnitOfWorkAsync : GenericUnitOfWorkAsync<IdentityEntities>, IUnitOfWorkAsync
    {
        private readonly ILoggerFactory _logger;
        public AudienceRepositoryAsync Audiences { get; private set; }
        public IGenericRepositoryAsync<tbl_AuthActivity> AuthActivity { get; private set; }
        public IGenericRepositoryAsync<tbl_AuthActivityAudience> AuthActivityAudiences { get; private set; }
        public IGenericRepositoryAsync<tbl_ChatConversation> ChatConversations { get; private set; }
        public IGenericRepositoryAsync<tbl_ChatFile> ChatFiles { get; private set; }
        public IGenericRepositoryAsync<tbl_ChatMessage> ChatMessages { get; private set; }
        public IGenericRepositoryAsync<tbl_ChatPrompt> ChatPrompts { get; private set; }
        public IGenericRepositoryAsync<tbl_Claim> Claims { get; private set; }
        public IGenericRepositoryAsync<tbl_EmailActivity> EmailActivity { get; private set; }
        public IGenericRepositoryAsync<tbl_EmailQueue> EmailQueue { get; private set; }
        public IGenericRepositoryAsync<tbl_Issuer> Issuers { get; private set; }
        public IGenericRepositoryAsync<tbl_Job> Jobs { get; private set; }
        public IGenericRepositoryAsync<tbl_JobSetting> JobSettings { get; private set; }
        public IGenericRepositoryAsync<tbl_LLMProvider> LLMProviders { get; private set; }
        public IGenericRepositoryAsync<tbl_LLMProviderSetting> LLMProviderSettings { get; private set; }
        public IGenericRepositoryAsync<tbl_LoginProvider> LoginProviders { get; private set; }
        public IGenericRepositoryAsync<tbl_Quote> Quotes { get; private set; }
        public IGenericRepositoryAsync<tbl_Refresh> Refreshes { get; private set; }
        public IGenericRepositoryAsync<tbl_Role> Roles { get; private set; }
        public IGenericRepositoryAsync<tbl_Setting> Settings { get; private set; }
        public IGenericRepositoryAsync<tbl_State> States { get; private set; }
        public IGenericRepositoryAsync<tbl_TextActivity> TextActivity { get; private set; }
        public IGenericRepositoryAsync<tbl_TextQueue> TextQueue { get; private set; }
        public IGenericRepositoryAsync<tbl_Url> Urls { get; private set; }
        public UserRepositoryAsync Users { get; private set; }

        public UnitOfWorkAsync(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWorkAsync(string connection, IContextService env)
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

            Audiences = new AudienceRepositoryAsync(context, env);
            AuthActivity = new GenericRepositoryAsync<tbl_AuthActivity>(context);
            AuthActivityAudiences = new GenericRepositoryAsync<tbl_AuthActivityAudience>(context);
            ChatConversations = new GenericRepositoryAsync<tbl_ChatConversation>(context);
            ChatFiles = new GenericRepositoryAsync<tbl_ChatFile>(context);
            ChatMessages = new GenericRepositoryAsync<tbl_ChatMessage>(context);
            ChatPrompts = new GenericRepositoryAsync<tbl_ChatPrompt>(context);
            Claims = new GenericRepositoryAsync<tbl_Claim>(context);
            EmailQueue = new GenericRepositoryAsync<tbl_EmailQueue>(context);
            EmailActivity = new GenericRepositoryAsync<tbl_EmailActivity>(context);
            Issuers = new GenericRepositoryAsync<tbl_Issuer>(context);
            Jobs = new GenericRepositoryAsync<tbl_Job>(context);
            JobSettings = new GenericRepositoryAsync<tbl_JobSetting>(context);
            LLMProviders = new GenericRepositoryAsync<tbl_LLMProvider>(context);
            LLMProviderSettings = new GenericRepositoryAsync<tbl_LLMProviderSetting>(context);
            LoginProviders = new GenericRepositoryAsync<tbl_LoginProvider>(context);
            Quotes = new GenericRepositoryAsync<tbl_Quote>(context);
            Refreshes = new GenericRepositoryAsync<tbl_Refresh>(context);
            Roles = new GenericRepositoryAsync<tbl_Role>(context);
            Settings = new GenericRepositoryAsync<tbl_Setting>(context);
            States = new GenericRepositoryAsync<tbl_State>(context);
            TextQueue = new GenericRepositoryAsync<tbl_TextQueue>(context);
            TextActivity = new GenericRepositoryAsync<tbl_TextActivity>(context);
            Urls = new GenericRepositoryAsync<tbl_Url>(context);
            Users = new UserRepositoryAsync(context, env);
        }

        public new void Dispose()
        {
            _logger?.Dispose();
            base.Dispose();
        }
    }
}
