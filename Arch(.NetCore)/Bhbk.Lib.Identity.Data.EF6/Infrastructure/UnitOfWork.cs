using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Repositories;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Identity.Data.EF6.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
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

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                case InstanceContext.IntegrationTest:
                    {
                        _context = new IdentityEntitiesFactory(connection).Create();
#if !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        _context = new IdentityEntitiesFactory(connection).Create();
#if !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.Configuration.LazyLoadingEnabled = false;
            _context.Configuration.ProxyCreationEnabled = true;

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
