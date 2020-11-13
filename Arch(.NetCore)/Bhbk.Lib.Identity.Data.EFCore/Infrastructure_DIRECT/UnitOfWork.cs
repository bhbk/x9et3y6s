using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IdentityEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public IGenericRepository<tbl_Claim> Claims { get; private set; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; private set; }
        public IGenericRepository<tbl_EmailActivity> EmailActivity { get; private set; }
        public IGenericRepository<tbl_Issuer> Issuers { get; private set; }
        public IGenericRepository<tbl_Login> Logins { get; private set; }
        public IGenericRepository<tbl_MOTD> MOTDs { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public IGenericRepository<tbl_Role> Roles { get; private set; }
        public IGenericRepository<tbl_Setting> Settings { get; private set; }
        public IGenericRepository<tbl_State> States { get; private set; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; private set; }
        public IGenericRepository<tbl_TextActivity> TextActivity { get; private set; }
        public IGenericRepository<tbl_Url> Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });

            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
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

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                case InstanceContext.UnitTest:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:")
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#elif RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:");
#endif
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
            Claims = new GenericRepository<tbl_Claim>(_context);
            EmailQueue = new GenericRepository<tbl_EmailQueue>(_context);
            EmailActivity = new GenericRepository<tbl_EmailActivity>(_context);
            Issuers = new GenericRepository<tbl_Issuer>(_context);
            Logins = new GenericRepository<tbl_Login>(_context);
            MOTDs = new GenericRepository<tbl_MOTD>(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new GenericRepository<tbl_Role>(_context);
            Settings = new GenericRepository<tbl_Setting>(_context);
            States = new GenericRepository<tbl_State>(_context);
            TextQueue = new GenericRepository<tbl_TextQueue>(_context);
            TextActivity = new GenericRepository<tbl_TextActivity>(_context);
            Urls = new GenericRepository<tbl_Url>(_context);
            Users = new UserRepository(_context, instance);
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
