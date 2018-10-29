using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Repository;
using Bhbk.Lib.Identity.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public class IdentityContext : IIdentityContext<AppDbContext>
    {
        private readonly AppDbContext _context;
        private readonly ContextType _situation;
        private readonly IMapper _convert;
        private readonly ActivityRepository _activityRepo;
        private readonly ClientRepository _clientRepo;
        private readonly ConfigRepository _configRepo;
        private readonly CustomRoleManager _customRoleMgr;
        private readonly CustomUserManager _customUserMgr;
        private readonly IssuerRepository _issuerRepo;
        private readonly LoginRepository _loginRepo;
        private UserQuotes _userQuote;

        public IdentityContext(DbContextOptions<AppDbContext> options, ContextType status)
            : this(new AppDbContext(options), status)
        {
        }

        public IdentityContext(DbContextOptionsBuilder<AppDbContext> optionsBuilder, ContextType status)
            : this(new AppDbContext(optionsBuilder.Options), status)
        {
        }

        private IdentityContext(AppDbContext context, ContextType status)
        {
            _disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _situation = status;
            _convert = new MapperConfiguration(config =>
            {
                config.AddProfile<IdentityMappings>();
            }).CreateMapper();

            _activityRepo = new ActivityRepository(_context);
            _clientRepo = new ClientRepository(_context);
            _configRepo = new ConfigRepository();
            _customRoleMgr = new CustomRoleManager(new CustomRoleStore(_context));
            _customUserMgr = new CustomUserManager(new CustomUserStore(_context));
            _issuerRepo = new IssuerRepository(_context);
            _loginRepo = new LoginRepository(_context);
        }

        public AppDbContext Context
        {
            get
            {
                return _context;
            }
        }

        public ContextType Situation
        {
            get
            {
                return _situation;
            }
        }

        public IMapper Convert
        {
            get
            {
                return _convert;
            }
        }

        public ActivityRepository ActivityRepo
        {
            get
            {
                return _activityRepo;
            }
        }

        public ClientRepository ClientRepo
        {
            get
            {
                return _clientRepo;
            }
        }

        public ConfigRepository ConfigRepo
        {
            get
            {
                return _configRepo;
            }
        }

        public CustomRoleManager CustomRoleMgr
        {
            get
            {
                return _customRoleMgr;
            }
        }

        public CustomUserManager CustomUserMgr
        {
            get
            {
                return _customUserMgr;
            }
        }

        public IssuerRepository IssuerRepo
        {
            get
            {
                return _issuerRepo;
            }
        }

        public LoginRepository LoginRepo
        {
            get
            {
                return _loginRepo;
            }
        }

        public UserQuotes UserQuote
        {
            get
            {
                return _userQuote;
            }
            set
            {
                _userQuote = value;
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
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
        // ~IdentityContext() {
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
