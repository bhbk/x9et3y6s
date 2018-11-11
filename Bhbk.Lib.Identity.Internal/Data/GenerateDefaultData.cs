using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data
{
    public class GenerateDefaultData
    {
        private readonly IIdentityContext<AppDbContext> _uow;

        public GenerateDefaultData(IIdentityContext<AppDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async Task CreateAsync()
        {
            IssuerCreate issuer;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                issuer = new IssuerCreate()
                {
                    Name = Strings.ApiDefaultIssuer,
                    IssuerKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = true,
                };

                foundIssuer = await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(issuer));

                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Strings.ApiDefaultClientApi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = Enums.ClientType.server.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientApi = await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(client));

                await _uow.CommitAsync();
            }

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Strings.ApiDefaultClientUi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = Enums.ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientUi = await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(client));

                await _uow.CommitAsync();
            }

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Strings.ApiDefaultLogin,
                    Immutable = true,
                };

                foundLogin = await _uow.LoginRepo.CreateAsync(_uow.Convert.Map<AppLogin>(login));

                await _uow.CommitAsync();
            }

            var foundUser = (await _uow.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).SingleOrDefault();

            if (foundUser == null)
            {
                user = new UserCreate()
                {
                    Email = Strings.ApiDefaultUserAdmin,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "Identity",
                    LastName = "System",
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = true,
                };

                await _uow.UserMgr.CreateAsync(_uow.Convert.Map<AppUser>(user), Strings.ApiDefaultUserPassword);

                foundUser = (await _uow.UserMgr.GetAsync(x => x.Email == user.Email)).Single();

                await _uow.UserMgr.Store.SetEmailConfirmedAsync(foundUser, true);
                await _uow.UserMgr.Store.SetPasswordConfirmedAsync(foundUser, true);
                await _uow.UserMgr.Store.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdminUi = (await _uow.RoleMgr.GetAsync(x => x.Name == Strings.ApiDefaultRoleForAdminUi)).SingleOrDefault();

            if (foundRoleForAdminUi == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientUi.Id,
                    Name = Strings.ApiDefaultRoleForAdminUi,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.RoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(role));
                await _uow.CommitAsync();

                foundRoleForAdminUi = (await _uow.RoleMgr.GetAsync(x => x.Name == role.Name)).Single();
            }

            var foundRoleForViewerApi = (await _uow.RoleMgr.GetAsync(x => x.Name == Strings.ApiDefaultRoleForViewerApi)).SingleOrDefault();

            if (foundRoleForViewerApi == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientApi.Id,
                    Name = Strings.ApiDefaultRoleForViewerApi,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.RoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(role));
                await _uow.CommitAsync();

                foundRoleForViewerApi = (await _uow.RoleMgr.GetAsync(x => x.Name == role.Name)).Single();
            }

            if (!await _uow.UserMgr.IsInLoginAsync(foundUser, Strings.ApiDefaultLogin))
                await _uow.UserMgr.AddLoginAsync(foundUser,
                    new UserLoginInfo(Strings.ApiDefaultLogin, Strings.ApiDefaultLoginKey, Strings.ApiDefaultLoginName));

            if (!await _uow.UserMgr.IsInRoleAsync(foundUser, foundRoleForAdminUi.Name))
                await _uow.UserMgr.AddToRoleAsync(foundUser, foundRoleForAdminUi.Name);

            if (!await _uow.UserMgr.IsInRoleAsync(foundUser, foundRoleForViewerApi.Name))
                await _uow.UserMgr.AddToRoleAsync(foundUser, foundRoleForViewerApi.Name);
        }

        public async Task DestroyAsync()
        {
            var user = await _uow.UserMgr.FindByNameAsync(Strings.ApiDefaultUserAdmin + "@local");

            if (user != null)
            {
                var roles = await _uow.UserMgr.GetRolesAsync(user);

                await _uow.UserMgr.RemoveFromRolesAsync(user, roles.ToArray());
                await _uow.UserMgr.DeleteAsync(user);
            }

            var roleAdmin = await _uow.RoleMgr.FindByNameAsync(Strings.ApiDefaultRoleForAdminUi);

            if (roleAdmin != null)
            {
                await _uow.RoleMgr.DeleteAsync(roleAdmin);
                await _uow.CommitAsync();
            }

            var roleViewer = await _uow.RoleMgr.FindByNameAsync(Strings.ApiDefaultRoleForViewerApi);

            if (roleViewer != null)
            {
                await _uow.RoleMgr.DeleteAsync(roleViewer);
                await _uow.CommitAsync();
            }

            var loginLocal = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (loginLocal != null)
            {
                await _uow.LoginRepo.DeleteAsync(loginLocal);
                await _uow.CommitAsync();
            }

            var clientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientApi)).SingleOrDefault();

            if (clientApi != null)
            {
                await _uow.ClientRepo.DeleteAsync(clientApi);
                await _uow.CommitAsync();
            }

            var clientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).SingleOrDefault();

            if (clientUi != null)
            {
                await _uow.ClientRepo.DeleteAsync(clientUi);
                await _uow.CommitAsync();
            }

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).SingleOrDefault();

            if (issuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(issuer);
                await _uow.CommitAsync();
            }
        }
    }
}
