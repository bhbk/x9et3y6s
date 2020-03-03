using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Data.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.EFCore.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
    {
#if !RELEASE
        private readonly ILoggerFactory _logger;
#endif
        private readonly IdentityEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public ClaimRepository Claims { get; private set; }
        public IssuerRepository Issuers { get; private set; }
        public LoginRepository Logins { get; private set; }
        public MOTDRepository MOTDs { get; private set; }
        public QueueEmailRepository QueueEmails { get; private set; }
        public QueueTextRepository QueueTexts { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public RoleRepository Roles { get; private set; }
        public SettingRepository Settings { get; private set; }
        public StateRepository States { get; private set; }
        public UrlRepository Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
#if !RELEASE
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });
#endif

            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection)
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#elif RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection);
#endif
                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                case InstanceContext.End2EndTest:
                case InstanceContext.IntegrationTest:
                case InstanceContext.UnitTest:
                    {
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:");

                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = instance.InstanceType;

            Activities = new ActivityRepository(_context);
            Audiences = new AudienceRepository(_context, instance);
            Claims = new ClaimRepository(_context);
            Issuers = new IssuerRepository(_context);
            Logins = new LoginRepository(_context);
            MOTDs = new MOTDRepository(_context);
            QueueEmails = new QueueEmailRepository(_context);
            QueueTexts = new QueueTextRepository(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new RoleRepository(_context);
            Settings = new SettingRepository(_context);
            States = new StateRepository(_context);
            Urls = new UrlRepository(_context);
            Users = new UserRepository(_context, instance);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _context.DisposeAsync();
        }
    }
}
