using AutoMapper;
using Bhbk.Lib.Core.UnitOfWork;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Repositories;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.UnitOfWork
{
    public class IdentityUnitOfWork : IIdentityUnitOfWork<IdentityDbContext>
    {
        private readonly ExecutionContext _situation;
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

        public IdentityUnitOfWork(DbContextOptions<IdentityDbContext> options, ExecutionContext situation, IConfigurationRoot conf, IMapper mapper)
            : this(new IdentityDbContext(options), situation, conf, mapper)
        {

        }

        public IdentityUnitOfWork(DbContextOptionsBuilder<IdentityDbContext> optionsBuilder, ExecutionContext situation, IConfigurationRoot conf, IMapper mapper)
            : this(new IdentityDbContext(optionsBuilder.Options), situation, conf, mapper)
        {

        }

        private IdentityUnitOfWork(IdentityDbContext context, ExecutionContext situation, IConfigurationRoot conf, IMapper shape)
        {
            _disposed = false;

            if (context == null)
                throw new ArgumentNullException();

            _context = context;
            _situation = situation;
            _shape = shape;

            _activityRepo = new ActivityRepository(context, situation, shape);
            _claimRepo = new ClaimRepository(context, situation, shape);
            _clientRepo = new ClientRepository(context, situation, conf, shape);
            _configRepo = new ConfigRepository(conf, situation);
            _issuerRepo = new IssuerRepository(context, situation, shape, conf["IdentityTenants:Salt"]);
            _loginRepo = new LoginRepository(context, situation, shape);
            _roleRepo = new RoleRepository(context, situation, shape);
            _refreshRepo = new RefreshRepository(context, situation, shape);
            _stateRepo = new StateRepository(context, situation, shape);
            _userRepo = new UserRepository(context, situation, conf, shape);
        }

        public ExecutionContext Situation
        {
            get
            {
                return _situation;
            }
        }

        public IMapper Shape
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
