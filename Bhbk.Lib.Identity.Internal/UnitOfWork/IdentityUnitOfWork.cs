using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Repositories;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.UnitOfWork
{
    public class IdentityUnitOfWork : IIdentityUnitOfWork, IDisposable
    {
        private readonly IdentityDbContext Context;
        public InstanceContext InstanceType { get; private set; }
        public LoggingLevel LoggingType { get; private set; }
        public IMapper Mapper { get; private set; }
        public ActivityRepository ActivityRepo { get; private set; }
        public ClaimRepository ClaimRepo { get; private set; }
        public ClientRepository ClientRepo { get; private set; }
        public ConfigRepository ConfigRepo { get; private set; }
        public IssuerRepository IssuerRepo { get; private set; }
        public LoginRepository LoginRepo { get; private set; }
        public RefreshRepository RefreshRepo { get; private set; }
        public RoleRepository RoleRepo { get; private set; }
        public StateRepository StateRepo { get; private set; }
        public UserRepository UserRepo { get; private set; }
        public QuotesModel UserQuote { get; set; }

        public IdentityUnitOfWork(DbContextOptions<IdentityDbContext> options, InstanceContext instance, IConfigurationRoot conf)
            : this(new IdentityDbContext(options), instance, conf)
        {

        }

        public IdentityUnitOfWork(DbContextOptionsBuilder<IdentityDbContext> optionsBuilder, InstanceContext instance, IConfigurationRoot conf)
            : this(new IdentityDbContext(optionsBuilder.Options), instance, conf)
        {

        }

        private IdentityUnitOfWork(IdentityDbContext context, InstanceContext instance, IConfigurationRoot conf)
        {
            Context = context ?? throw new ArgumentNullException();
            InstanceType = instance;
            LoggingType = (LoggingLevel)Enum.Parse(typeof(LoggingLevel), conf["Logging:LogLevel:Default"], true);
            Mapper = new MapperConfiguration(x =>
            {
                x.AddProfile<MapperProfile>();
            }).CreateMapper();

            ActivityRepo = new ActivityRepository(Context, InstanceType, Mapper);
            ClaimRepo = new ClaimRepository(Context, InstanceType, Mapper);
            ClientRepo = new ClientRepository(Context, InstanceType, Mapper, conf);
            ConfigRepo = new ConfigRepository(InstanceType, conf);
            IssuerRepo = new IssuerRepository(Context, InstanceType, Mapper, conf["IdentityTenants:Salt"]);
            LoginRepo = new LoginRepository(Context, InstanceType, Mapper);
            RoleRepo = new RoleRepository(Context, InstanceType, Mapper);
            RefreshRepo = new RefreshRepository(Context, InstanceType, Mapper);
            StateRepo = new StateRepository(Context, InstanceType, Mapper);
            UserRepo = new UserRepository(Context, InstanceType, Mapper, conf);
        }

        public async Task CommitAsync()
        {
            await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
