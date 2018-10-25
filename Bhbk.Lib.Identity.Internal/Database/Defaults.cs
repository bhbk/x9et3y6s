using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Database
{
    public class Defaults
    {
        private readonly IIdentityContext _ioc;

        public Defaults(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
        }

        public async void Create()
        {
            AudienceCreate audience;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            var foundClient = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiDefaultClient).SingleOrDefault();

            if (foundClient == null)
            {
                client = new ClientCreate()
                {
                    Name = Strings.ApiDefaultClient,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(client).Devolve());
                foundClient = _ioc.ClientMgmt.Store.Get(x => x.Name == client.Name).Single();
            }

            var foundAudienceApi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiDefaultAudienceApi).SingleOrDefault();

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

                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(audience).Devolve());
                foundAudienceApi = _ioc.AudienceMgmt.Store.Get(x => x.Name == audience.Name).Single();
            }

            var foundAudienceUi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiDefaultAudienceUi).SingleOrDefault();

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

                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(audience).Devolve());
                foundAudienceUi = _ioc.AudienceMgmt.Store.Get(x => x.Name == audience.Name).Single();
            }

            var foundUser = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).SingleOrDefault();

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

                await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(user).Devolve(), Strings.ApiDefaultUserPassword);
                foundUser = _ioc.UserMgmt.Store.Get(x => x.Email == user.Email).Single();

                await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(foundUser, true);
                await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(foundUser, true);
                await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdminUi = _ioc.RoleMgmt.Store.Get(x => x.Name == Strings.ApiDefaultRoleForAdminUi).SingleOrDefault();

            if (foundRoleForAdminUi == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudienceUi.Id,
                    Name = Strings.ApiDefaultRoleForAdminUi,
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(role).Devolve());
                foundRoleForAdminUi = _ioc.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundRoleForViewerApi = _ioc.RoleMgmt.Store.Get(x => x.Name == Strings.ApiDefaultRoleForViewerApi).SingleOrDefault();

            if (foundRoleForViewerApi == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudienceApi.Id,
                    Name = Strings.ApiDefaultRoleForViewerApi,
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(role).Devolve());
                foundRoleForViewerApi = _ioc.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundLogin = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Strings.ApiDefaultLogin).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Strings.ApiDefaultLogin,
                    Immutable = true,
                };

                await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(login).Devolve());
                foundLogin = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == login.LoginProvider).SingleOrDefault();
            }

            if (!await _ioc.UserMgmt.IsInLoginAsync(foundUser, Strings.ApiDefaultLogin))
                await _ioc.UserMgmt.AddLoginAsync(foundUser,
                    new UserLoginInfo(Strings.ApiDefaultLogin, Strings.ApiDefaultLoginKey, Strings.ApiDefaultLoginName));

            if (!await _ioc.UserMgmt.IsInRoleAsync(foundUser, foundRoleForAdminUi.Name))
                await _ioc.UserMgmt.AddToRoleAsync(foundUser, foundRoleForAdminUi.Name);

            if (!await _ioc.UserMgmt.IsInRoleAsync(foundUser, foundRoleForViewerApi.Name))
                await _ioc.UserMgmt.AddToRoleAsync(foundUser, foundRoleForViewerApi.Name);
        }

        public async void Destroy()
        {
            var user = await _ioc.UserMgmt.FindByNameAsync(Strings.ApiDefaultUserAdmin + "@local");

            if (user != null)
            {
                var roles = await _ioc.UserMgmt.GetRolesAsync(user);

                await _ioc.UserMgmt.RemoveFromRolesAsync(user, roles.ToArray());
                await _ioc.UserMgmt.DeleteAsync(user);
            }

            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Strings.ApiDefaultLogin).SingleOrDefault();

            if (login != null)
                await _ioc.LoginMgmt.DeleteAsync(login);

            var roleAdmin = await _ioc.RoleMgmt.FindByNameAsync(Strings.ApiDefaultRoleForAdminUi);

            if (roleAdmin != null)
                await _ioc.RoleMgmt.DeleteAsync(roleAdmin);

            var roleViewer = await _ioc.RoleMgmt.FindByNameAsync(Strings.ApiDefaultRoleForViewerApi);

            if (roleViewer != null)
                await _ioc.RoleMgmt.DeleteAsync(roleViewer);

            var audienceApi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiDefaultAudienceApi).SingleOrDefault();

            if (audienceApi != null)
                await _ioc.AudienceMgmt.DeleteAsync(audienceApi);

            var audienceUi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiDefaultAudienceUi).SingleOrDefault();

            if (audienceUi != null)
                await _ioc.AudienceMgmt.DeleteAsync(audienceUi);

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiDefaultClient).SingleOrDefault();

            if (client != null)
                await _ioc.ClientMgmt.DeleteAsync(client);
        }
    }
}
