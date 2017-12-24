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
        private bool _disposed;
        private ContextType _status;
        private AppDbContext _context;
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

            _audienceMgmt = new AudienceManager(new AudienceStore(_context));
            _clientMgmt = new ClientManager(new ClientStore(_context));
            _configMgmt = new ConfigManager(new ConfigStore());
            _loginMgmt = new LoginManager(new LoginStore(_context));
            _roleMgmt = new CustomRoleManager(new CustomRoleStore(_context));
            _userMgmt = new CustomUserManager(new CustomUserStore(_context));
        }

        public ContextType ContextStatus
        {
            get
            {
                return _status;
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

        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
