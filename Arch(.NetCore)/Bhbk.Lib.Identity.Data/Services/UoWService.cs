using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Repositories;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Data.Services
{
    public class UoWService : IUoWService, IDisposable
    {
        private readonly IdentityEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public ActivityRepository_Deprecate Activities_Deprecate { get; private set; }
        public ClaimRepository Claims { get; private set; }
        public ClientRepository Clients { get; private set; }
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

        public UoWService(IConfiguration conf)
            : this(conf, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UoWService(IConfiguration conf, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(conf["Databases:IdentityEntities"]);
#elif !RELEASE
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .UseSqlServer(conf["Databases:IdentityEntities"])
                            .EnableSensitiveDataLogging();
#endif
                        _context = new IdentityEntities(builder.Options);
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        var builder = new DbContextOptionsBuilder<IdentityEntities>()
                            .EnableSensitiveDataLogging();
                        InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(builder, ":InMemory:");

                        _context = new IdentityEntities(builder.Options);
                        //_context = Create.MockedDbContextFor<IdentityEntities>();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            InstanceType = instance.InstanceType;

            Activities = new ActivityRepository(_context, instance.InstanceType);
            Activities_Deprecate = new ActivityRepository_Deprecate(_context, instance.InstanceType);
            Claims = new ClaimRepository(_context, instance.InstanceType);
            Clients = new ClientRepository(_context, instance.InstanceType);
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
