using Bhbk.Lib.Identity.Manager;
using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Repository;
using Bhbk.Lib.Identity.Store;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public partial class UnitOfWork : IUnitOfWork
    {
        private bool _disposed = false;
        private CustomIdentityDbContext _context;
        private CustomRoleManager _roleMgmt;
        private CustomUserManager _userMgmt;
        private IGenericRepository<AppAudience, Guid> _audienceRepo;
        private IGenericRepository<AppClient, Guid> _clientRepo;
        private IGenericRepository<AppRealm, Guid> _realmRepo;
        private IGenericRepository<AppRole, Guid> _roleRepo;
        private IGenericRepository<AppUser, Guid> _userRepo;

        public UnitOfWork()
            : this(new CustomIdentityDbContext())
        {
            _audienceRepo = new AudienceRepository(_context);
            _clientRepo = new ClientRepository(_context);
            _realmRepo = new RealmRepository(_context);
            _roleRepo = new RoleRepository(_context);
            _userRepo = new UserRepository(_context);

            _userMgmt = new CustomUserManager(new CustomUserStore(_context));
            _roleMgmt = new CustomRoleManager(new CustomRoleStore(_context));
        }

        public UnitOfWork(CustomIdentityDbContext context)
        {
            _context = context;

            _audienceRepo = new AudienceRepository(_context);
            _clientRepo = new ClientRepository(_context);
            _realmRepo = new RealmRepository(_context);
            _roleRepo = new RoleRepository(_context);
            _userRepo = new UserRepository(_context);

            _userMgmt = new CustomUserManager(new CustomUserStore(_context));
            _roleMgmt = new CustomRoleManager(new CustomRoleStore(_context));
        }

        public CustomIdentityDbContext CustomContext
        {
            get
            {
                if (this._context == null)
                    this._context = new CustomIdentityDbContext();
                
                return this._context;
            }
            set
            {
                this._context = value;
            }
        }

        public static UnitOfWork Create()
        {
            return new UnitOfWork();
        }

        public static UnitOfWork Create(CustomIdentityDbContext context)
        {
            return new UnitOfWork(context);
        }

        public IGenericRepository<AppAudience, Guid> AudienceRepository
        {
            get
            {
                if (this._audienceRepo == null)
                    this._audienceRepo = new AudienceRepository(_context);

                return this._audienceRepo;
            }
        }

        public IGenericRepository<AppClient, Guid> ClientRepository
        {
            get
            {
                if (this._clientRepo == null)
                    this._clientRepo = new ClientRepository(_context);

                return this._clientRepo;
            }
        }

        public IGenericRepository<AppRealm, Guid> RealmRepository
        {
            get
            {
                if (this._realmRepo == null)
                    this._realmRepo = new RealmRepository(_context);

                return this._realmRepo;
            }
        }

        public IGenericRepository<AppRole, Guid> RoleRepository
        {
            get
            {
                if (this._roleRepo == null)
                    this._roleRepo = new RoleRepository(_context);

                return this._roleRepo;
            }
        }

        public IGenericRepository<AppUser, Guid> UserRepository
        {
            get
            {
                if (this._userRepo == null)
                    this._userRepo = new UserRepository(_context);

                return this._userRepo;
            }
        }

        public CustomUserManager CustomUserManager
        {
            get
            {
                if (_userMgmt == null)
                {
                    _userMgmt = new CustomUserManager(new CustomUserStore(_context));

                    //create custom token provider...
                    _userMgmt.UserTokenProvider =
                        new DataProtectorTokenProvider<AppUser, Guid>(new CustomDataProtection().Create("UserToken"))
                        {
                            TokenLifespan = TimeSpan.FromHours(Statics.ApiDefaultTokenExpire)
                        };

                    //create custom password validator...
                    _userMgmt.PasswordValidator = new CustomPasswordValidator
                    {
                        RequiredLength = Statics.ApiDefaultPassMinLength
                    };
                }

                return _userMgmt;
            }
        }

        public CustomRoleManager CustomRoleManager
        {
            get
            {
                if (_roleMgmt == null)
                    _roleMgmt = new CustomRoleManager(new CustomRoleStore(_context));

                return _roleMgmt;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public Task SaveAsync()
        {
            _context.SaveChangesAsync();

            return Task.FromResult(true);
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
