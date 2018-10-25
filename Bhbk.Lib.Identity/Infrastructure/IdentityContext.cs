using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Repository;
using Bhbk.Lib.Identity.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public class IdentityContext : IIdentityContext<AppDbContext>
    {
        private readonly ContextType _situation;
        private readonly AppDbContext _context;
        private readonly ActivityRepository _activityRepo;
        private readonly AudienceRepository _audienceRepo;
        private readonly ConfigRepository _configRepo;
        private readonly CustomRoleManager _customRoleMgr;
        private readonly CustomUserManager _customUserMgr;
        private readonly ClientRepository _clientRepo;
        private readonly LoginRepository _loginRepo;
        private UserQuoteOfDay _userQuote;

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

            _activityRepo = new ActivityRepository(_context);
            _audienceRepo = new AudienceRepository(_context);
            _clientRepo = new ClientRepository(_context);
            _configRepo = new ConfigRepository();
            _customRoleMgr = new CustomRoleManager(new CustomRoleStore(_context));
            _customUserMgr = new CustomUserManager(new CustomUserStore(_context));
            _loginRepo = new LoginRepository(_context);
        }

        public ContextType Situation
        {
            get
            {
                return _situation;
            }
        }

        public AppDbContext Context
        {
            get
            {
                return _context;
            }
        }

        public ActivityRepository ActivityRepo
        {
            get
            {
                return _activityRepo;
            }
        }

        public AudienceRepository AudienceRepo
        {
            get
            {
                return _audienceRepo;
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

        public LoginRepository LoginRepo
        {
            get
            {
                return _loginRepo;
            }
        }

        public UserQuoteOfDay UserQuote
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

        public async void DefaultsCreate()
        {
            AudienceCreate audience;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            var foundClient = (await ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).SingleOrDefault();

            if (foundClient == null)
            {
                client = new ClientCreate()
                {
                    Name = Strings.ApiDefaultClient,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = true,
                };

                foundClient = await ClientRepo.CreateAsync(new ClientFactory<ClientCreate>(client).ToStore());

                await CommitAsync();
            }

            var foundAudienceApi = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceApi)).SingleOrDefault();

            if (foundAudienceApi == null)
            {
                audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Strings.ApiDefaultAudienceApi,
                    AudienceType = Enums.AudienceType.server.ToString(),
                    Enabled = true,
                    Immutable = true,
                };
                
                foundAudienceApi = await AudienceRepo.CreateAsync(new AudienceFactory<AudienceCreate>(audience).ToStore());

                await CommitAsync();
            }

            var foundAudienceUi = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).SingleOrDefault();

            if (foundAudienceUi == null)
            {
                audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Strings.ApiDefaultAudienceUi,
                    AudienceType = Enums.AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = true,
                };
                
                foundAudienceUi = await AudienceRepo.CreateAsync(new AudienceFactory<AudienceCreate>(audience).ToStore());

                await CommitAsync();
            }

            var foundLogin = (await LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Strings.ApiDefaultLogin,
                    Immutable = true,
                };

                foundLogin = await LoginRepo.CreateAsync(new LoginFactory<LoginCreate>(login).ToStore());

                await CommitAsync();
            }

            var foundUser = CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).SingleOrDefault();

            if (foundUser == null)
            {
                user = new UserCreate()
                {
                    Email = Strings.ApiDefaultUserAdmin,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "Identity",
                    LastName = "Admin",
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = true,
                };

                await CustomUserMgr.CreateAsync(new UserFactory<UserCreate>(user).ToStore(), Strings.ApiDefaultUserPassword);

                foundUser = CustomUserMgr.Store.Get(x => x.Email == user.Email).Single();

                await CustomUserMgr.Store.SetEmailConfirmedAsync(foundUser, true);
                await CustomUserMgr.Store.SetPasswordConfirmedAsync(foundUser, true);
                await CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdminUi = CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiDefaultRoleForAdminUi).SingleOrDefault();

            if (foundRoleForAdminUi == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudienceUi.Id,
                    Name = Strings.ApiDefaultRoleForAdminUi,
                    Enabled = true,
                    Immutable = true,
                };

                await CustomRoleMgr.CreateAsync(new RoleFactory<RoleCreate>(role).ToStore());
                await CommitAsync();

                foundRoleForAdminUi = CustomRoleMgr.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundRoleForViewerApi = CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiDefaultRoleForViewerApi).SingleOrDefault();

            if (foundRoleForViewerApi == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudienceApi.Id,
                    Name = Strings.ApiDefaultRoleForViewerApi,
                    Enabled = true,
                    Immutable = true,
                };

                await CustomRoleMgr.CreateAsync(new RoleFactory<RoleCreate>(role).ToStore());
                await CommitAsync();

                foundRoleForViewerApi = CustomRoleMgr.Store.Get(x => x.Name == role.Name).Single();
            }

            if (!await CustomUserMgr.IsInLoginAsync(foundUser, Strings.ApiDefaultLogin))
                await CustomUserMgr.AddLoginAsync(foundUser,
                    new UserLoginInfo(Strings.ApiDefaultLogin, Strings.ApiDefaultLoginKey, Strings.ApiDefaultLoginName));

            if (!await CustomUserMgr.IsInRoleAsync(foundUser, foundRoleForAdminUi.Name))
                await CustomUserMgr.AddToRoleAsync(foundUser, foundRoleForAdminUi.Name);

            if (!await CustomUserMgr.IsInRoleAsync(foundUser, foundRoleForViewerApi.Name))
                await CustomUserMgr.AddToRoleAsync(foundUser, foundRoleForViewerApi.Name);
        }

        public async void DefautsDestroy()
        {
            var user = await CustomUserMgr.FindByNameAsync(Strings.ApiDefaultUserAdmin + "@local");

            if (user != null)
            {
                var roles = await CustomUserMgr.GetRolesAsync(user);

                await CustomUserMgr.RemoveFromRolesAsync(user, roles.ToArray());
                await CustomUserMgr.DeleteAsync(user);
            }

            var roleAdmin = await CustomRoleMgr.FindByNameAsync(Strings.ApiDefaultRoleForAdminUi);

            if (roleAdmin != null)
            {
                await CustomRoleMgr.DeleteAsync(roleAdmin);
                await CommitAsync();
            }

            var roleViewer = await CustomRoleMgr.FindByNameAsync(Strings.ApiDefaultRoleForViewerApi);

            if (roleViewer != null)
            {
                await CustomRoleMgr.DeleteAsync(roleViewer);
                await CommitAsync();
            }

            var loginLocal = (await LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (loginLocal != null)
            {
                await LoginRepo.DeleteAsync(loginLocal);
                await CommitAsync();
            }

            var audienceApi = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceApi)).SingleOrDefault();

            if (audienceApi != null)
            {
                await AudienceRepo.DeleteAsync(audienceApi);
                await CommitAsync();
            }

            var audienceUi = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).SingleOrDefault();

            if (audienceUi != null)
            {
                await AudienceRepo.DeleteAsync(audienceUi);
                await CommitAsync();
            }

            var client = (await ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).SingleOrDefault();

            if (client != null)
            {
                await ClientRepo.DeleteAsync(client);
                await CommitAsync();
            }
        }

        public async void TestsCreate()
        {
            if (_situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            AppLogin login;
            AppUser user;
            AppRole role;
            AppAudience audience;

            //create clients
            await ClientRepo.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = Strings.ApiUnitTestClient1,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }).ToStore());

            await ClientRepo.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = Strings.ApiUnitTestClient2,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }).ToStore());

            await CommitAsync();

            //create audiences
            await AudienceRepo.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = (await ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single().Id,
                    Name = Strings.ApiUnitTestAudience1,
                    AudienceType = Enums.AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).ToStore());

            await AudienceRepo.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = (await ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single().Id,
                    Name = Strings.ApiUnitTestAudience2,
                    AudienceType = Enums.AudienceType.server.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).ToStore());

            await CommitAsync();

            //assign uris to audiences
            audience = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();

            await AudienceRepo.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Strings.ApiUnitTestUri1,
                    AbsoluteUri = Strings.ApiUnitTestUri1Link,
                    Enabled = true,
                }).ToStore());

            audience = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience2)).Single();

            await AudienceRepo.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Strings.ApiUnitTestUri2,
                    AbsoluteUri = Strings.ApiUnitTestUri2Link,
                    Enabled = true,
                }).ToStore());

            await CommitAsync();

            //create logins
            await LoginRepo.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin1,
                    Immutable = false
                }).ToStore());

            await LoginRepo.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin2,
                    Immutable = false
                }).ToStore());

            await CommitAsync();

            //create roles
            await CustomRoleMgr.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single().Id,
                    Name = Strings.ApiUnitTestRole1,
                    Enabled = true,
                    Immutable = false
                }).ToStore());

            await CustomRoleMgr.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = (await AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience2)).Single().Id,
                    Name = Strings.ApiUnitTestRole2,
                    Enabled = true,
                    Immutable = false
                }).ToStore());

            await CommitAsync();

            //create user A
            await CustomUserMgr.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser1,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }).ToStore(), Strings.ApiUnitTestUserPassCurrent);

            user = CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            await CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);
            await CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);
            await CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            //create user B
            await CustomUserMgr.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser2,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    Immutable = false,
                }).ToStore(), Strings.ApiUnitTestUserPassCurrent);

            user = CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser2).Single();

            await CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);
            await CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);
            await CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            //assign roles, claims & logins to user A
            user = CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            role = CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            login = (await LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            await CustomUserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await CustomUserMgr.IsInRoleAsync(user, role.Name))
                await CustomUserMgr.AddToRoleAsync(user, role.Name);

            if (!await CustomUserMgr.IsInLoginAsync(user, login.LoginProvider))
                await CustomUserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign roles, claims & logins to user B
            user = CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser2).Single();
            role = CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            login = (await LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin2)).Single();

            await CustomUserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await CustomUserMgr.IsInRoleAsync(user, role.Name))
                await CustomUserMgr.AddToRoleAsync(user, role.Name);

            if (!await CustomUserMgr.IsInLoginAsync(user, login.LoginProvider))
                await CustomUserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));
        }

        public async void TestsCreateRandom(uint sets)
        {
            if (_situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                AppLogin login;
                AppUser user;
                AppRole role;
                AppAudience audience;

                var clientName = Strings.ApiUnitTestClient1 + "-" + RandomValues.CreateBase64String(4);
                var audienceName = Strings.ApiUnitTestAudience1 + "-" + RandomValues.CreateBase64String(4);
                var loginName = Strings.ApiUnitTestLogin1 + "-" + RandomValues.CreateBase64String(4);
                var roleName = Strings.ApiUnitTestRole1 + "-" + RandomValues.CreateBase64String(4);
                var userName = RandomValues.CreateAlphaNumericString(4) + "-" + Strings.ApiUnitTestUser1;
                var uriName = Strings.ApiUnitTestUri1 + "-" + RandomValues.CreateBase64String(4);

                //create random client
                await ClientRepo.CreateAsync(new ClientFactory<ClientCreate>(
                    new ClientCreate()
                    {
                        Name = clientName,
                        ClientKey = RandomValues.CreateBase64String(32),
                        Enabled = true,
                        Immutable = false,
                    }).ToStore());

                await CommitAsync();

                //create random audience
                await AudienceRepo.CreateAsync(new AudienceFactory<AudienceCreate>(
                    new AudienceCreate()
                    {
                        ClientId = (await ClientRepo.GetAsync(x => x.Name == clientName)).Single().Id,
                        Name = audienceName,
                        AudienceType = Enums.AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }).ToStore());

                await CommitAsync();

                //assign uris to random audience
                audience = (await AudienceRepo.GetAsync(x => x.Name == audienceName)).Single();

                await AudienceRepo.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                    new AudienceUriCreate()
                    {
                        AudienceId = audience.Id,
                        Name = uriName,
                        AbsoluteUri = Strings.ApiUnitTestUri1Link,
                        Enabled = true,
                    }).ToStore());

                await CommitAsync();

                //create random login
                await LoginRepo.CreateAsync(new LoginFactory<LoginCreate>(
                    new LoginCreate()
                    {
                        LoginProvider = loginName,
                        Immutable = false
                    }).ToStore());

                await CommitAsync();

                //create random role
                await CustomRoleMgr.CreateAsync(new RoleFactory<RoleCreate>(
                    new RoleCreate()
                    {
                        AudienceId = (await AudienceRepo.GetAsync(x => x.Name == audienceName)).Single().Id,
                        Name = roleName,
                        Enabled = true,
                        Immutable = false
                    }).ToStore());

                await CommitAsync();

                //create random user
                await CustomUserMgr.CreateAsync(new UserFactory<UserCreate>(
                    new UserCreate()
                    {
                        Email = userName,
                        PhoneNumber = Strings.ApiDefaultPhone,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }).ToStore(), Strings.ApiUnitTestUserPassCurrent);

                //assign roles, claims & logins to random user
                user = CustomUserMgr.Store.Get(x => x.Email == userName).Single();
                role = CustomRoleMgr.Store.Get(x => x.Name == roleName).Single();
                login = (await LoginRepo.GetAsync(x => x.LoginProvider == loginName)).Single();

                await CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);
                await CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);
                await CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);
                await CustomUserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

                if (!await CustomUserMgr.IsInRoleAsync(user, role.Name))
                    await CustomUserMgr.AddToRoleAsync(user, role.Name);

                if (!await CustomUserMgr.IsInLoginAsync(user, login.LoginProvider))
                    await CustomUserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));
            }
        }

        public async void TestsDestroy()
        {
            if (_situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            foreach (var user in CustomUserMgr.Store.Get())
                await CustomUserMgr.DeleteAsync(user);

            foreach (var role in CustomRoleMgr.Store.Get())
                await CustomRoleMgr.DeleteAsync(role);

            await CommitAsync();

            foreach (var login in await LoginRepo.GetAsync())
                await LoginRepo.DeleteAsync(login);

            await CommitAsync();

            foreach (var audience in await AudienceRepo.GetAsync())
                await AudienceRepo.DeleteAsync(audience);

            await CommitAsync();

            foreach (var client in await ClientRepo.GetAsync())
                await ClientRepo.DeleteAsync(client);

            await CommitAsync();
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
