using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Repositories;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Identity.Data.EF6.Services
{
    public class UoWService : IUoWService, IDisposable
    {
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

        public UoWService(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UoWService(string connection, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if RELEASE

#elif !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                        _context = new IdentityContextFactory(connection).Create();
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        _context = new IdentityContextFactory(connection).Create();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.Configuration.LazyLoadingEnabled = false;
            _context.Configuration.ProxyCreationEnabled = true;

            InstanceType = instance.InstanceType;

            Activities = new ActivityRepository(_context, instance.InstanceType);
            Audiences = new AudienceRepository(_context, instance.InstanceType);
            Claims = new ClaimRepository(_context, instance.InstanceType);
            Issuers = new IssuerRepository(_context, instance.InstanceType);
            Logins = new LoginRepository(_context, instance.InstanceType);
            MOTDs = new MOTDRepository(_context, instance.InstanceType);
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
