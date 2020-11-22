using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_TBL;
using Bhbk.Lib.Identity.Data.EF6.Repositories_TBL;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Identity.Data.EF6.Infrastructure_TBL
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IdentityEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public AudienceRepository Audiences { get; private set; }
        public IGenericRepository<tbl_Claim> Claims { get; private set; }
        public IGenericRepository<tbl_EmailQueue> EmailQueue { get; private set; }
        public IGenericRepository<tbl_Issuer> Issuers { get; private set; }
        public IGenericRepository<tbl_Login> Logins { get; private set; }
        public IGenericRepository<tbl_MOTD> MOTDs { get; private set; }
        public RefreshRepository Refreshes { get; private set; }
        public IGenericRepository<tbl_Role> Roles { get; private set; }
        public IGenericRepository<tbl_Setting> Settings { get; private set; }
        public IGenericRepository<tbl_State> States { get; private set; }
        public IGenericRepository<tbl_TextQueue> TextQueue { get; private set; }
        public IGenericRepository<tbl_Url> Urls { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
                        _context = new IdentityEntitiesFactory(connection).Create();
#if !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                    }
                    break;

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        var memory = Effort.EntityConnectionFactory.CreateTransient(connection);

                        _context = new IdentityEntitiesFactory(memory).Create();
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
            Claims = new GenericRepository<tbl_Claim>(_context);
            EmailQueue = new GenericRepository<tbl_EmailQueue>(_context);
            Issuers = new GenericRepository<tbl_Issuer>(_context);
            Logins = new GenericRepository<tbl_Login>(_context);
            MOTDs = new GenericRepository<tbl_MOTD>(_context);
            Refreshes = new RefreshRepository(_context);
            Roles = new GenericRepository<tbl_Role>(_context);
            Settings = new GenericRepository<tbl_Setting>(_context);
            States = new GenericRepository<tbl_State>(_context);
            TextQueue = new GenericRepository<tbl_TextQueue>(_context);
            Urls = new GenericRepository<tbl_Url>(_context);
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
