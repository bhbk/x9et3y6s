using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public class IdentityContext : IIdentityContext<AppDbContext>
    {
        private readonly ContextType _situation;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly ActivityRepository _activityRepo;
        private readonly ClientRepository _clientRepo;
        private readonly ConfigRepository _configRepo;
        private readonly IssuerRepository _issuerRepo;
        private readonly LoginRepository _loginRepo;
        private readonly RoleRepository _roleRepo;
        private readonly UserRepository _userRepo;
        private UserQuotes _userQuote;

        public IdentityContext(DbContextOptions<AppDbContext> options, ContextType situation, IConfigurationRoot conf, IMapper mapper)
            : this(new AppDbContext(options), situation, conf, mapper)
        {

        }

        public IdentityContext(DbContextOptionsBuilder<AppDbContext> optionsBuilder, ContextType situation, IConfigurationRoot conf, IMapper mapper)
            : this(new AppDbContext(optionsBuilder.Options), situation, conf, mapper)
        {

        }

        private IdentityContext(AppDbContext context, ContextType situation, IConfigurationRoot conf, IMapper mapper)
        {
            _disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _situation = situation;
            _mapper = mapper;

            _activityRepo = new ActivityRepository(_context, _situation, _mapper);
            _clientRepo = new ClientRepository(_context, _situation, conf, _mapper);
            _configRepo = new ConfigRepository(conf, _situation);
            _issuerRepo = new IssuerRepository(_context, _situation, mapper, conf["IdentityTenants:Salt"]);
            _loginRepo = new LoginRepository(_context, _situation, mapper);
            _roleRepo = new RoleRepository(_context, _situation, mapper);
            _userRepo = new UserRepository(_context, _situation, conf, mapper);
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

        public IMapper Transform
        {
            get
            {
                return _mapper;
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

        public RoleRepository RoleRepo
        {
            get
            {
                return _roleRepo;
            }
        }

        public UserRepository UserRepo
        {
            get
            {
                return _userRepo;
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
