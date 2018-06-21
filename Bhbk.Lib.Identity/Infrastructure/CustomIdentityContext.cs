using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Stores;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public class CustomIdentityContext : IIdentityContext
    {
        private ContextType _status;
        private readonly AppDbContext _context;
        private ActivityStore _activity;
        private AudienceManager _audienceMgmt;
        private ClientManager _clientMgmt;
        private ConfigManager _configMgmt;
        private CustomRoleManager _roleMgmt;
        private CustomUserManager _userMgmt;
        private LoginManager _loginMgmt;
        private UserQuoteOfDay _userQuote;

        public CustomIdentityContext(DbContextOptions<AppDbContext> options)
            : this(new AppDbContext(options))
        {
        }

        public CustomIdentityContext(DbContextOptionsBuilder<AppDbContext> optionsBuilder)
            : this(new AppDbContext(optionsBuilder.Options))
        {
        }

        private CustomIdentityContext(AppDbContext context)
        {
            _disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            _context = context;

            _activity = new ActivityStore(_context);
            _audienceMgmt = new AudienceManager(new AudienceStore(_context));
            _clientMgmt = new ClientManager(new ClientStore(_context));
            _configMgmt = new ConfigManager(new ConfigStore());
            _loginMgmt = new LoginManager(new LoginStore(_context));
            _roleMgmt = new CustomRoleManager(new CustomRoleStore(_context));
            _userMgmt = new CustomUserManager(new CustomUserStore(_context));
        }

        public AppDbContext GetContext()
        {
            return _context;
        }

        public ContextType ContextStatus
        {
            get
            {
                return _status;
            }
        }

        public ActivityStore Activity
        {
            get
            {
                if (_activity == null)
                    _activity = new ActivityStore(_context);

                return _activity;
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

        public LoginManager LoginMgmt
        {
            get
            {
                if (_loginMgmt == null)
                    _loginMgmt = new LoginManager(new LoginStore(_context));

                return _loginMgmt;
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

        public UserQuoteOfDay UserQuote
        {
            get
            {
                if (_userQuote == null)
                    _userQuote = new UserQuoteOfDay();

                return _userQuote;
            }
            set
            {
                _userQuote = value;
            }
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CustomFitchnealContext() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
