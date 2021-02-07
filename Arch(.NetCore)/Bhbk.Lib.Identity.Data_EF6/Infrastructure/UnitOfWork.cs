using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Data_EF6.Repositories;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Identity.Data_EF6.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IdentityEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public AuthActivityRepository AuthActivity { get; private set; }
        public IGenericRepository<E_Claim> Claims { get; private set; }
        public IGenericRepository<E_EmailQueue> EmailQueue { get; private set; }
        public IGenericRepository<E_Issuer> Issuers { get; private set; }
        public IGenericRepository<E_Login> Logins { get; private set; }
        public IGenericRepository<E_MOTD> MOTDs { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public IGenericRepository<E_Role> Roles { get; private set; }
        public IGenericRepository<E_Setting> Settings { get; private set; }
        public IGenericRepository<E_State> States { get; private set; }
        public IGenericRepository<E_TextQueue> TextQueue { get; private set; }
        public IGenericRepository<E_Url> Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService env)
        {
            switch (env.InstanceType)
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

            InstanceType = env.InstanceType;

            Audiences = new AudienceRepository(_context, env);
            AuthActivity = new AuthActivityRepository(_context);
            Claims = new GenericRepository<E_Claim>(_context);
            EmailQueue = new GenericRepository<E_EmailQueue>(_context);
            Issuers = new GenericRepository<E_Issuer>(_context);
            Logins = new GenericRepository<E_Login>(_context);
            MOTDs = new GenericRepository<E_MOTD>(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new GenericRepository<E_Role>(_context);
            Settings = new GenericRepository<E_Setting>(_context);
            States = new GenericRepository<E_State>(_context);
            TextQueue = new GenericRepository<E_TextQueue>(_context);
            Urls = new GenericRepository<E_Url>(_context);
            Users = new UserRepository(_context, env);
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
