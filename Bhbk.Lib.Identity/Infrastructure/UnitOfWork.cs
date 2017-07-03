using Bhbk.Lib.Identity.Interface;
using Bhbk.Lib.Identity.Manager;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using System;
using System.Data.Entity.Core.EntityClient;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private ContextType _contextType;
        private CustomIdentityDbContext _context;
        private AudienceManager _audienceMgmt;
        private ClientManager _clientMgmt;
        private ProviderManager _providerMgmt;
        private CustomRoleManager _roleMgmt;
        private ConfigManager _configMgmt;
        private CustomUserManager _userMgmt;

        public UnitOfWork()
            : this(new CustomIdentityDbContext())
        {
            this._contextType = ContextType.Live;
        }

        public UnitOfWork(EntityConnection connection)
            : this(new CustomIdentityDbContext(connection))
        {
            if (connection == null)
                throw new ArgumentNullException();

            this._contextType = ContextType.UnitTest;
        }

        private UnitOfWork(CustomIdentityDbContext context)
        {
            this._disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            this._context = context;

            CreateManagers();
        }

        public static UnitOfWork Create()
        {
            return new UnitOfWork();
        }

        private void CreateManagers()
        {
            _audienceMgmt = new AudienceManager(new AudienceStore(this._context));
            _clientMgmt = new ClientManager(new ClientStore(this._context));
            _configMgmt = new ConfigManager(new ConfigStore());
            _providerMgmt = new ProviderManager(new ProviderStore(this._context));
            _roleMgmt = new CustomRoleManager(new CustomRoleStore(this._context));
            _userMgmt = new CustomUserManager(new CustomUserStore(this._context));
        }

        public ContextType ContextStatus
        {
            get
            {
                return this._contextType;
            }
        }

        public AudienceManager AudienceMgmt
        {
            get
            {
                if (_audienceMgmt == null)
                    _audienceMgmt = new AudienceManager(new AudienceStore(_context));

                return _audienceMgmt;
            }
        }

        public ClientManager ClientMgmt
        {
            get
            {
                if (_clientMgmt == null)
                    _clientMgmt = new ClientManager(new ClientStore(_context));

                return _clientMgmt;
            }
        }

        public ConfigManager ConfigMgmt
        {
            get
            {
                if (_configMgmt == null)
                    _configMgmt = new ConfigManager(new ConfigStore());

                return _configMgmt;
            }
        }

        public ProviderManager ProviderMgmt
        {
            get
            {
                if (_providerMgmt == null)
                    _providerMgmt = new ProviderManager(new ProviderStore(_context));

                return _providerMgmt;
            }
        }

        public CustomRoleManager RoleMgmt
        {
            get
            {
                if (_roleMgmt == null)
                    _roleMgmt = new CustomRoleManager(new CustomRoleStore(_context));

                return _roleMgmt;
            }
        }

        public CustomUserManager UserMgmt
        {
            get
            {
                if (_userMgmt == null)
                    _userMgmt = new CustomUserManager(new CustomUserStore(_context));

                return _userMgmt;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                    _context.Dispose();
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
