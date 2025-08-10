using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Data.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Identity.Data.EF.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IdentityEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public AuthActivityRepository AuthActivity { get; private set; }
        public IGenericRepository<tbl_AuthActivityAudience> AuthActivityAudiences { get; private set; }
        public IGenericRepository<tbl_Claim> Claims { get; private set; }
        public IGenericRepository<tbl_EmailActivity> EmailActivity { get; private set; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; private set; }
        public IssuerRepository Issuers { get; private set; }
        public LoginRepository Logins { get; private set; }
        public IGenericRepository<tbl_MOTD> MOTDs { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public RoleRepository Roles { get; private set; }
        public IGenericRepository<tbl_Setting> Settings { get; private set; }
        public IGenericRepository<tbl_State> States { get; private set; }
        public IGenericRepository<tbl_TextActivity> TextActivity { get; private set; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; private set; }
        public IGenericRepository<tbl_Url> Urls { get; private set; }
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

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:")
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#else
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseInMemoryDatabase(":InMemory:");
#endif
                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        throw new NotImplementedException();
                    }

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = env.InstanceType;

            Audiences = new AudienceRepository(_context, env);
            AuthActivity = new AuthActivityRepository(_context);
            AuthActivityAudiences = new GenericRepository<tbl_AuthActivityAudience>(_context);
            Claims = new GenericRepository<tbl_Claim>(_context);
            EmailQueue = new GenericRepository<tbl_EmailQueue>(_context);
            EmailActivity = new GenericRepository<tbl_EmailActivity>(_context);
            Issuers = new IssuerRepository(_context);
            Logins = new LoginRepository(_context);
            MOTDs = new GenericRepository<tbl_MOTD>(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new RoleRepository(_context);
            Settings = new GenericRepository<tbl_Setting>(_context);
            States = new GenericRepository<tbl_State>(_context);
            TextQueue = new GenericRepository<tbl_TextQueue>(_context);
            TextActivity = new GenericRepository<tbl_TextActivity>(_context);
            Urls = new GenericRepository<tbl_Url>(_context);
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
