using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Services
{
    public class UoWService : IUoWService, IDisposable
    {
        private readonly _DbContext _context;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository ActivityRepo { get; private set; }
        public ClaimRepository ClaimRepo { get; private set; }
        public ClientRepository ClientRepo { get; private set; }
        public IssuerRepository IssuerRepo { get; private set; }
        public LoginRepository LoginRepo { get; private set; }
        public RefreshRepository RefreshRepo { get; private set; }
        public RoleRepository RoleRepo { get; private set; }
        public SettingRepository SettingRepo { get; private set; }
        public StateRepository StateRepo { get; private set; }
        public UserRepository UserRepo { get; private set; }

        public UoWService(IConfiguration conf)
            : this(conf, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UoWService(IConfiguration conf, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if RELEASE
                        var options = new DbContextOptionsBuilder<_DbContext>()
                            .UseSqlServer(conf["Databases:IdentityEntities"]);
#elif !RELEASE
                        var options = new DbContextOptionsBuilder<_DbContext>()
                            .UseSqlServer(conf["Databases:IdentityEntities"])
                            .EnableSensitiveDataLogging();
#endif
                        _context = new _DbContext(options);
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        var options = new DbContextOptionsBuilder<_DbContext>()
                            .EnableSensitiveDataLogging();

                        InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

                        _context = new _DbContext(options);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            InstanceType = instance.InstanceType;

            ActivityRepo = new ActivityRepository(_context, instance.InstanceType);
            ClaimRepo = new ClaimRepository(_context, instance.InstanceType);
            ClientRepo = new ClientRepository(_context, instance.InstanceType);
            IssuerRepo = new IssuerRepository(_context, instance.InstanceType, conf["IdentityTenants:Salt"]);
            LoginRepo = new LoginRepository(_context, instance.InstanceType);
            RoleRepo = new RoleRepository(_context, instance.InstanceType);
            RefreshRepo = new RefreshRepository(_context, instance.InstanceType);
            SettingRepo = new SettingRepository(_context, instance.InstanceType);
            StateRepo = new StateRepository(_context, instance.InstanceType);
            UserRepo = new UserRepository(_context, instance.InstanceType);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
