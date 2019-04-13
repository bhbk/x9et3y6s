using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public class IdentityUnitOfWork : IIdentityUnitOfWork<IdentityDbContext>
    {
        private readonly ExecutionType _situation;
        private readonly IMapper _shape;
        private readonly IdentityDbContext _context;
        private readonly ActivityRepository _activityRepo;
        private readonly ClaimRepository _claimRepo;
        private readonly ClientRepository _clientRepo;
        private readonly ConfigRepository _configRepo;
        private readonly IssuerRepository _issuerRepo;
        private readonly LoginRepository _loginRepo;
        private readonly RefreshRepository _refreshRepo;
        private readonly RoleRepository _roleRepo;
        private readonly StateRepository _stateRepo;
        private readonly UserRepository _userRepo;
        private Quotes _userQuote;

        public IdentityUnitOfWork(DbContextOptions<IdentityDbContext> options, ExecutionType situation, IConfigurationRoot conf, IMapper mapper)
            : this(new IdentityDbContext(options), situation, conf, mapper)
        {

        }

        public IdentityUnitOfWork(DbContextOptionsBuilder<IdentityDbContext> optionsBuilder, ExecutionType situation, IConfigurationRoot conf, IMapper mapper)
            : this(new IdentityDbContext(optionsBuilder.Options), situation, conf, mapper)
        {

        }

        private IdentityUnitOfWork(IdentityDbContext context, ExecutionType situation, IConfigurationRoot conf, IMapper shape)
        {
            _disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _situation = situation;
            _shape = shape;

            _activityRepo = new ActivityRepository(_context, _situation, _shape);
            _claimRepo = new ClaimRepository(_context, _situation, _shape);
            _clientRepo = new ClientRepository(_context, _situation, conf, _shape);
            _configRepo = new ConfigRepository(conf, _situation);
            _issuerRepo = new IssuerRepository(_context, _situation, shape, conf["IdentityTenants:Salt"]);
            _loginRepo = new LoginRepository(_context, _situation, shape);
            _roleRepo = new RoleRepository(_context, _situation, shape);
            _refreshRepo = new RefreshRepository(_context, _situation, shape);
            _stateRepo = new StateRepository(_context, _situation, _shape);
            _userRepo = new UserRepository(_context, _situation, conf, shape);
        }

        public ExecutionType Situation
        {
            get
            {
                return _situation;
            }
        }

        public IMapper Reshape
        {
            get
            {
                return _shape;
            }
        }

        public ActivityRepository ActivityRepo
        {
            get
            {
                return _activityRepo;
            }
        }

        public ClaimRepository ClaimRepo
        {
            get
            {
                return _claimRepo;
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

        public RefreshRepository RefreshRepo
        {
            get
            {
                return _refreshRepo;
            }
        }

        public RoleRepository RoleRepo
        {
            get
            {
                return _roleRepo;
            }
        }

        public StateRepository StateRepo
        {
            get
            {
                return _stateRepo;
            }
        }

        public UserRepository UserRepo
        {
            get
            {
                return _userRepo;
            }
        }

        public Quotes UserQuote
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
