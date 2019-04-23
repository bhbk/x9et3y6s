using AutoMapper;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Repositories;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.UnitOfWork
{
    public class IdentityUnitOfWork : IIdentityUnitOfWork<IdentityDbContext>, IDisposable
    {
        private readonly InstanceContext _instance;
        private readonly LoggingLevel _logger;
        private readonly IMapper _mapper;
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

        public IdentityUnitOfWork(DbContextOptions<IdentityDbContext> options, InstanceContext instance, IConfigurationRoot conf)
            : this(new IdentityDbContext(options), instance, conf)
        {

        }

        public IdentityUnitOfWork(DbContextOptionsBuilder<IdentityDbContext> optionsBuilder, InstanceContext instance, IConfigurationRoot conf)
            : this(new IdentityDbContext(optionsBuilder.Options), instance, conf)
        {

        }

        private IdentityUnitOfWork(IdentityDbContext context, InstanceContext instance, IConfigurationRoot conf)
        {
            _context = context ?? throw new ArgumentNullException();
            _instance = instance;
            _logger = (LoggingLevel)Enum.Parse(typeof(LoggingLevel), conf["Logging:LogLevel:Default"], true);
            _mapper = new MapperConfiguration(x =>
            {
                x.AddProfile<AutoMapperProfile>();
            }).CreateMapper();

            _activityRepo = new ActivityRepository(_context, _instance, _mapper);
            _claimRepo = new ClaimRepository(_context, _instance, _mapper);
            _clientRepo = new ClientRepository(_context, _instance, conf, _mapper);
            _configRepo = new ConfigRepository(conf, _instance);
            _issuerRepo = new IssuerRepository(_context, _instance, _mapper, conf["IdentityTenants:Salt"]);
            _loginRepo = new LoginRepository(_context, _instance, _mapper);
            _roleRepo = new RoleRepository(_context, _instance, _mapper);
            _refreshRepo = new RefreshRepository(_context, _instance, _mapper);
            _stateRepo = new StateRepository(_context, _instance, _mapper);
            _userRepo = new UserRepository(_context, _instance, conf, _mapper);
        }

        public InstanceContext InstanceType
        {
            get { return _instance; }
        }

        public LoggingLevel LoggingType
        {
            get { return _logger; }
        }

        public IMapper Mapper
        {
            get { return _mapper; }
        }

        public ActivityRepository ActivityRepo
        {
            get { return _activityRepo; }
        }

        public ClaimRepository ClaimRepo
        {
            get { return _claimRepo; }
        }

        public ClientRepository ClientRepo
        {
            get { return _clientRepo; }
        }

        public ConfigRepository ConfigRepo
        {
            get { return _configRepo; }
        }

        public IssuerRepository IssuerRepo
        {
            get { return _issuerRepo; }
        }

        public LoginRepository LoginRepo
        {
            get { return _loginRepo; }
        }

        public RefreshRepository RefreshRepo
        {
            get { return _refreshRepo; }
        }

        public RoleRepository RoleRepo
        {
            get { return _roleRepo; }
        }

        public StateRepository StateRepo
        {
            get { return _stateRepo; }
        }

        public UserRepository UserRepo
        {
            get { return _userRepo; }
        }

        public Quotes UserQuote
        {
            get { return _userQuote; }
            set {  _userQuote = value; }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
