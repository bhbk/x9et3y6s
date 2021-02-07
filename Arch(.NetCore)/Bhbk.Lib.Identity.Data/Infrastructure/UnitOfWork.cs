using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Repositories;
//using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Identity.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IdentityEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public AuthActivityRepository AuthActivity { get; private set; }
        public ClaimRepository Claims { get; private set; }
        public EmailActivityRepository EmailActivity { get; private set; }
        public EmailQueueRepository EmailQueue { get; private set; }
        public IssuerRepository Issuers { get; private set; }
        public LoginRepository Logins { get; private set; }
        public MOTDRepository MOTDs { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public RoleRepository Roles { get; private set; }
        public SettingRepository Settings { get; private set; }
        public StateRepository States { get; private set; }
        public TextActivityRepository TextActivity { get; private set; }
        public TextQueueRepository TextQueue { get; private set; }
        public UrlRepository Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService env)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });

            switch (env.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection)
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#else
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection);
#endif
                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        //_context = Create.MockedDbContextFor<IdentityEntities>();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = env.InstanceType;

            Audiences = new AudienceRepository(_context, env);
            AuthActivity = new AuthActivityRepository(_context);
            Claims = new ClaimRepository(_context);
            EmailActivity = new EmailActivityRepository(_context);
            EmailQueue = new EmailQueueRepository(_context);
            Issuers = new IssuerRepository(_context);
            Logins = new LoginRepository(_context);
            MOTDs = new MOTDRepository(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new RoleRepository(_context);
            Settings = new SettingRepository(_context);
            States = new StateRepository(_context);
            TextActivity = new TextActivityRepository(_context);
            TextQueue = new TextQueueRepository(_context);
            Urls = new UrlRepository(_context);
            Users = new UserRepository(_context, env);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _logger.Dispose();
            _context.Dispose();
        }
    }
}
