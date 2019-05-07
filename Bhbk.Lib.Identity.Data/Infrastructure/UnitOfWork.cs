using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Helpers;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly _DbContext _context;
        public InstanceContext InstanceType { get; private set; }
        public IMapper Mapper { get; private set; }
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

        public UnitOfWork(DbContextOptionsBuilder<_DbContext> optionsBuilder, IConfiguration conf)
            : this(new _DbContext(optionsBuilder.Options), conf, InstanceContext.DeployedOrLocal)
        {

        }

        public UnitOfWork(DbContextOptionsBuilder<_DbContext> optionsBuilder, IConfiguration conf, InstanceContext instance)
            : this(new _DbContext(optionsBuilder.Options), conf, instance)
        {

        }

        private UnitOfWork(_DbContext context, IConfiguration conf, InstanceContext instance)
        {
            _context = context ?? throw new ArgumentNullException();

            InstanceType = instance;
            Mapper = new MapperConfiguration(x =>
            {
                x.AddProfile<MapperProfile>();
            }).CreateMapper();            

            ActivityRepo = new ActivityRepository(context, instance);
            ClaimRepo = new ClaimRepository(context, instance);
            ClientRepo = new ClientRepository(context, instance);
            IssuerRepo = new IssuerRepository(context, instance, conf["IdentityTenants:Salt"]);
            LoginRepo = new LoginRepository(context, instance);
            RoleRepo = new RoleRepository(context, instance);
            RefreshRepo = new RefreshRepository(context, instance);
            SettingRepo = new SettingRepository(context, instance);
            StateRepo = new StateRepository(context, instance);
            UserRepo = new UserRepository(context, instance);
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
