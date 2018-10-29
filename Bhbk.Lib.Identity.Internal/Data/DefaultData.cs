using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.Lib.Identity.Data
{
    public class DefaultData
    {
        private readonly IIdentityContext<AppDbContext> _uow;

        public DefaultData(IIdentityContext<AppDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async void Create()
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

            var foundUser = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).SingleOrDefault();

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

                await _uow.CustomUserMgr.CreateAsync(_uow.Convert.Map<AppUser>(user), Strings.ApiDefaultUserPassword);

                foundUser = _uow.CustomUserMgr.Store.Get(x => x.Email == user.Email).Single();

                await _uow.CustomUserMgr.Store.SetEmailConfirmedAsync(foundUser, true);
                await _uow.CustomUserMgr.Store.SetPasswordConfirmedAsync(foundUser, true);
                await _uow.CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdminUi = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiDefaultRoleForAdminUi).SingleOrDefault();

            if (foundRoleForAdminUi == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientUi.Id,
                    Name = Strings.ApiDefaultRoleForAdminUi,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.CustomRoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(role));
                await _uow.CommitAsync();

                foundRoleForAdminUi = _uow.CustomRoleMgr.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundRoleForViewerApi = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiDefaultRoleForViewerApi).SingleOrDefault();

            if (foundRoleForViewerApi == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientApi.Id,
                    Name = Strings.ApiDefaultRoleForViewerApi,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.CustomRoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(role));
                await _uow.CommitAsync();

                foundRoleForViewerApi = _uow.CustomRoleMgr.Store.Get(x => x.Name == role.Name).Single();
            }

            if (!await _uow.CustomUserMgr.IsInLoginAsync(foundUser, Strings.ApiDefaultLogin))
                await _uow.CustomUserMgr.AddLoginAsync(foundUser,
                    new UserLoginInfo(Strings.ApiDefaultLogin, Strings.ApiDefaultLoginKey, Strings.ApiDefaultLoginName));

            if (!await _uow.CustomUserMgr.IsInRoleAsync(foundUser, foundRoleForAdminUi.Name))
                await _uow.CustomUserMgr.AddToRoleAsync(foundUser, foundRoleForAdminUi.Name);

            if (!await _uow.CustomUserMgr.IsInRoleAsync(foundUser, foundRoleForViewerApi.Name))
                await _uow.CustomUserMgr.AddToRoleAsync(foundUser, foundRoleForViewerApi.Name);
        }

        public async void Destroy()
        {
            var user = await _uow.CustomUserMgr.FindByNameAsync(Strings.ApiDefaultUserAdmin + "@local");

            if (user != null)
            {
                var roles = await _uow.CustomUserMgr.GetRolesAsync(user);

                await _uow.CustomUserMgr.RemoveFromRolesAsync(user, roles.ToArray());
                await _uow.CustomUserMgr.DeleteAsync(user);
            }

            var roleAdmin = await _uow.CustomRoleMgr.FindByNameAsync(Strings.ApiDefaultRoleForAdminUi);

            if (roleAdmin != null)
            {
                await _uow.CustomRoleMgr.DeleteAsync(roleAdmin);
                await _uow.CommitAsync();
            }

            var roleViewer = await _uow.CustomRoleMgr.FindByNameAsync(Strings.ApiDefaultRoleForViewerApi);

            if (roleViewer != null)
            {
                await _uow.CustomRoleMgr.DeleteAsync(roleViewer);
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
