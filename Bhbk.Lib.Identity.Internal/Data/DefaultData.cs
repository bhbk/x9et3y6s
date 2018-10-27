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
            AudienceCreate audience;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            var foundClient = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).SingleOrDefault();

            if (foundClient == null)
            {
                client = new ClientCreate()
                {
                    Name = Strings.ApiDefaultClient,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = true,
                };

                foundClient = await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(client));

                await _uow.CommitAsync();
            }

            var foundAudienceApi = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceApi)).SingleOrDefault();

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

                foundAudienceApi = await _uow.AudienceRepo.CreateAsync(_uow.Convert.Map<AppAudience>(audience));

                await _uow.CommitAsync();
            }

            var foundAudienceUi = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).SingleOrDefault();

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

                foundAudienceUi = await _uow.AudienceRepo.CreateAsync(_uow.Convert.Map<AppAudience>(audience));

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
                    LastName = "Admin",
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
                    AudienceId = foundAudienceUi.Id,
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
                    AudienceId = foundAudienceApi.Id,
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

            var audienceApi = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceApi)).SingleOrDefault();

            if (audienceApi != null)
            {
                await _uow.AudienceRepo.DeleteAsync(audienceApi);
                await _uow.CommitAsync();
            }

            var audienceUi = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).SingleOrDefault();

            if (audienceUi != null)
            {
                await _uow.AudienceRepo.DeleteAsync(audienceUi);
                await _uow.CommitAsync();
            }

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).SingleOrDefault();

            if (client != null)
            {
                await _uow.ClientRepo.DeleteAsync(client);
                await _uow.CommitAsync();
            }
        }
    }
}
