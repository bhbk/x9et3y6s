using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Data.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Services
{
    public class UoWService : IUoWService, IDisposable
    {
        private readonly IdentityEntities _context;
        private readonly ILoggerFactory _logger;

        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public ClaimRepository Claims { get; private set; }
        public IssuerRepository Issuers { get; private set; }
        public LoginRepository Logins { get; private set; }
        public MotDRepository MOTDs { get; private set; }
        public QueueEmailRepository QueueEmails { get; private set; }
        public QueueTextRepository QueueTexts { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public RoleRepository Roles { get; private set; }
        public SettingRepository Settings { get; private set; }
        public StateRepository States { get; private set; }
        public UrlRepository Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UoWService(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UoWService(string connection, IContextService instance)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });

            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection);
#elif !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(connection)
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#endif
                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:")
                            .EnableSensitiveDataLogging();

                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = instance.InstanceType;

            Activities = new ActivityRepository(_context, instance.InstanceType);
            Audiences = new AudienceRepository(_context, instance.InstanceType);
            Claims = new ClaimRepository(_context, instance.InstanceType);
            Issuers = new IssuerRepository(_context, instance.InstanceType);
            Logins = new LoginRepository(_context, instance.InstanceType);
            MOTDs = new MotDRepository(_context, instance.InstanceType);
            QueueEmails = new QueueEmailRepository(_context, instance.InstanceType);
            QueueTexts = new QueueTextRepository(_context, instance.InstanceType);
            Refreshes = new RefreshRepository(_context, instance.InstanceType);
            Roles = new RoleRepository(_context, instance.InstanceType);
            Settings = new SettingRepository(_context, instance.InstanceType);
            States = new StateRepository(_context, instance.InstanceType);
            Urls = new UrlRepository(_context, instance.InstanceType);
            Users = new UserRepository(_context, instance.InstanceType);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
