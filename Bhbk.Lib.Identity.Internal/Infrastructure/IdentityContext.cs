using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public class IdentityContext : IIdentityContext<AppDbContext>
    {
        private readonly ContextType _situation;
        private readonly IMapper _convert;
        private readonly AppDbContext _context;
        private readonly ActivityRepository _activityRepo;
        private readonly ClientRepository _clientRepo;
        private readonly ConfigRepository _configRepo;
        private readonly IssuerRepository _issuerRepo;
        private readonly LoginRepository _loginRepo;
        private readonly RoleManagerExt _roleMgr;
        private readonly UserManagerExt _userMgr;
        private UserQuotes _userQuote;

        public IdentityContext(DbContextOptions<AppDbContext> options, ContextType situation, IConfigurationRoot conf)
            : this(new AppDbContext(options, conf), situation, conf)
        {

        }

        public IdentityContext(DbContextOptionsBuilder<AppDbContext> optionsBuilder, ContextType situation, IConfigurationRoot conf)
            : this(new AppDbContext(optionsBuilder.Options, conf), situation, conf)
        {

        }

        private IdentityContext(AppDbContext context, ContextType situation, IConfigurationRoot conf)
        {
            _disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _situation = situation;

            _convert = new MapperConfiguration(x =>
            {
                x.AddProfile<IdentityMappings>();
            }).CreateMapper();

            _activityRepo = new ActivityRepository(_context);
            _clientRepo = new ClientRepository(_context);
            _configRepo = new ConfigRepository(conf);
            _issuerRepo = new IssuerRepository(_context);
            _loginRepo = new LoginRepository(_context);
            _roleMgr = new RoleManagerExt(new RoleStoreExt(_context));
            _userMgr = new UserManagerExt(new UserStoreExt(_context));

            _issuerRepo.Salt = conf["IdentityTenants:Salt"];
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

        public RoleManagerExt RoleMgr
        {
            get
            {
                return _roleMgr;
            }
        }

        public UserManagerExt UserMgr
        {
            get
            {
                return _userMgr;
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
