using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models_TSQL;
using Bhbk.Lib.Identity.Data_EF6.Repositories_TSQL;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Identity.Data_EF6.Infrastructure_TSQL
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IdentityEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public IGenericRepository<uvw_Claim> Claims { get; private set; }
        public IGenericRepository<uvw_EmailQueue> EmailQueue { get; private set; }
        public IGenericRepository<uvw_Issuer> Issuers { get; private set; }
        public IGenericRepository<uvw_Login> Logins { get; private set; }
        public IGenericRepository<uvw_MOTD> MOTDs { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public IGenericRepository<uvw_Role> Roles { get; private set; }
        public IGenericRepository<uvw_Setting> Settings { get; private set; }
        public IGenericRepository<uvw_State> States { get; private set; }
        public IGenericRepository<uvw_TextQueue> TextQueue { get; private set; }
        public IGenericRepository<uvw_Url> Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                case InstanceContext.SystemTest:
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
                        throw new NotImplementedException();
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
            Claims = new GenericRepository<uvw_Claim>(_context);
            EmailQueue = new GenericRepository<uvw_EmailQueue>(_context);
            Issuers = new GenericRepository<uvw_Issuer>(_context);
            Logins = new GenericRepository<uvw_Login>(_context);
            MOTDs = new GenericRepository<uvw_MOTD>(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new GenericRepository<uvw_Role>(_context);
            Settings = new GenericRepository<uvw_Setting>(_context);
            States = new GenericRepository<uvw_State>(_context);
            TextQueue = new GenericRepository<uvw_TextQueue>(_context);
            Urls = new GenericRepository<uvw_Url>(_context);
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
    }
}
