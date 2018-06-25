using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Helpers
{
    public class DatasetHelper
    {
        private readonly IIdentityContext _ioc;

        public DatasetHelper(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
        }

        public async void CreateDefaultData()
        {
            AudienceCreate audience;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            var foundClient = _ioc.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (foundClient == null)
            {
                client = new ClientCreate()
                {
                    Name = Statics.ApiDefaultClient,
                    ClientKey = CryptoHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(client).Devolve());
                foundClient = _ioc.ClientMgmt.Store.Get(x => x.Name == client.Name).Single();
            }

            var foundAudienceApi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudienceApi).SingleOrDefault();

            if (foundAudienceApi == null)
            {
                audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Statics.ApiDefaultAudienceApi,
                    AudienceType = AudienceType.server.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(audience).Devolve());
                foundAudienceApi = _ioc.AudienceMgmt.Store.Get(x => x.Name == audience.Name).Single();
            }

            var foundAudienceUi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudienceUi).SingleOrDefault();

            if (foundAudienceUi == null)
            {
                audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Statics.ApiDefaultAudienceUi,
                    AudienceType = AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(audience).Devolve());
                foundAudienceUi = _ioc.AudienceMgmt.Store.Get(x => x.Name == audience.Name).Single();
            }

            var foundUser = _ioc.UserMgmt.Store.Get(x => x.Email == Statics.ApiDefaultUserAdmin).SingleOrDefault();

            if (foundUser == null)
            {
                user = new UserCreate()
                {
                    Email = Statics.ApiDefaultUserAdmin,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "Identity",
                    LastName = "Admin",
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = true,
                };

                await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(user).Devolve(), BaseLib.Statics.ApiDefaultUserPassword);
                foundUser = _ioc.UserMgmt.Store.Get(x => x.Email == user.Email).Single();

                await _ioc.UserMgmt.SetEmailConfirmedAsync(foundUser, true);
                await _ioc.UserMgmt.SetPasswordConfirmedAsync(foundUser, true);
                await _ioc.UserMgmt.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdminUi = _ioc.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForAdminUi).SingleOrDefault();

            if (foundRoleForAdminUi == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudienceUi.Id,
                    Name = Statics.ApiDefaultRoleForAdminUi,
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(role).Devolve());
                foundRoleForAdminUi = _ioc.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundRoleForViewerApi = _ioc.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForViewerApi).SingleOrDefault();

            if (foundRoleForViewerApi == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudienceApi.Id,
                    Name = Statics.ApiDefaultRoleForViewerApi,
                    Enabled = true,
                    Immutable = true,
                };

                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(role).Devolve());
                foundRoleForViewerApi = _ioc.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundLogin = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Statics.ApiDefaultLogin,
                    Immutable = true,
                };

                await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(login).Devolve());
                foundLogin = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == login.LoginProvider).SingleOrDefault();
            }

            if (!await _ioc.UserMgmt.IsInLoginAsync(foundUser, Statics.ApiDefaultLogin))
                await _ioc.UserMgmt.AddLoginAsync(foundUser,
                    new UserLoginInfo(Statics.ApiDefaultLogin, Statics.ApiDefaultLoginKey, Statics.ApiDefaultLoginName));

            if (!await _ioc.UserMgmt.IsInRoleAsync(foundUser, foundRoleForAdminUi.Name))
                await _ioc.UserMgmt.AddToRoleAsync(foundUser, foundRoleForAdminUi.Name);

            if (!await _ioc.UserMgmt.IsInRoleAsync(foundUser, foundRoleForViewerApi.Name))
                await _ioc.UserMgmt.AddToRoleAsync(foundUser, foundRoleForViewerApi.Name);
        }

        public async void CreateTestData()
        {
            AppLogin login;
            AppUser user;
            AppRole role;

            //create clients
            await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = BaseLib.Statics.ApiUnitTestClientA,
                    ClientKey = CryptoHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = BaseLib.Statics.ApiUnitTestClientB,
                    ClientKey = CryptoHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            //create audiences
            await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single().Id,
                    Name = BaseLib.Statics.ApiUnitTestAudienceA,
                    AudienceType = AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientB).Single().Id,
                    Name = BaseLib.Statics.ApiUnitTestAudienceB,
                    AudienceType = AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            //create roles
            await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single().Id,
                    Name = BaseLib.Statics.ApiUnitTestRoleA,
                    Enabled = true,
                    Immutable = false
                }).Devolve());

            await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single().Id,
                    Name = BaseLib.Statics.ApiUnitTestRoleB,
                    Enabled = true,
                    Immutable = false
                }).Devolve());

            //create logins
            await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Statics.ApiUnitTestLoginA,
                    Immutable = false
                }).Devolve());

            await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Statics.ApiUnitTestLoginB,
                    Immutable = false
                }).Devolve());

            //create user A
            await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = BaseLib.Statics.ApiUnitTestUserA,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "First " + CryptoHelper.GenerateRandomBase64(4),
                    LastName = "Last " + CryptoHelper.GenerateRandomBase64(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);

            user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            await _ioc.UserMgmt.SetEmailConfirmedAsync(user, true);
            await _ioc.UserMgmt.SetPasswordConfirmedAsync(user, true);
            await _ioc.UserMgmt.SetPhoneNumberConfirmedAsync(user, true);

            //create user B
            await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = BaseLib.Statics.ApiUnitTestUserB,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "First " + CryptoHelper.GenerateRandomBase64(4),
                    LastName = "Last " + CryptoHelper.GenerateRandomBase64(4),
                    LockoutEnabled = false,
                    Immutable = false,
                }).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);

            user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserB).Single();

            await _ioc.UserMgmt.SetEmailConfirmedAsync(user, true);
            await _ioc.UserMgmt.SetPasswordConfirmedAsync(user, true);
            await _ioc.UserMgmt.SetPhoneNumberConfirmedAsync(user, true);

            //assign roles, claims & logins to user A
            user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            await _ioc.UserMgmt.AddClaimAsync(user, new Claim(BaseLib.Statics.ApiUnitTestClaimType, BaseLib.Statics.ApiUnitTestClaimValue));

            if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign roles, claims & logins to user B
            user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserB).Single();
            role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginB).Single();

            await _ioc.UserMgmt.AddClaimAsync(user, new Claim(BaseLib.Statics.ApiUnitTestClaimType, BaseLib.Statics.ApiUnitTestClaimValue));

            if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));
        }

        public async void CreateTestDataRandom()
        {
            for(int i = 0; i < 20; i++)
            {
                AppLogin login;
                AppUser user;
                AppRole role;

                var clientName = BaseLib.Statics.ApiUnitTestClientA + "-" + CryptoHelper.GenerateRandomBase64(4);
                var audienceName = BaseLib.Statics.ApiUnitTestAudienceA + "-" + CryptoHelper.GenerateRandomBase64(4);
                var roleName = BaseLib.Statics.ApiUnitTestRoleA + "-" + CryptoHelper.GenerateRandomBase64(4);
                var loginName = BaseLib.Statics.ApiUnitTestLoginA + "-" + CryptoHelper.GenerateRandomBase64(4);
                var userName = CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestUserA;

                //create random client
                await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                    new ClientCreate()
                    {
                        Name = clientName,
                        ClientKey = CryptoHelper.GenerateRandomBase64(32),
                        Enabled = true,
                        Immutable = false,
                    }).Devolve());

                //create random audience
                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                    new AudienceCreate()
                    {
                        ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == clientName).Single().Id,
                        Name = audienceName,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }).Devolve());

                //create random role
                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                    new RoleCreate()
                    {
                        AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == audienceName).Single().Id,
                        Name = roleName,
                        Enabled = true,
                        Immutable = false
                    }).Devolve());

                //create random login
                await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                    new LoginCreate()
                    {
                        LoginProvider = loginName,
                        Immutable = false
                    }).Devolve());

                //create random user
                await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                    new UserCreate()
                    {
                        Email = userName,
                        PhoneNumber = Statics.ApiDefaultPhone,
                        FirstName = "First " + CryptoHelper.GenerateRandomBase64(4),
                        LastName = "Last " + CryptoHelper.GenerateRandomBase64(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);

                //assign roles, claims & logins to random user
                user = _ioc.UserMgmt.Store.Get(x => x.Email == userName).Single();
                role = _ioc.RoleMgmt.Store.Get(x => x.Name == roleName).Single();
                login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == loginName).Single();

                await _ioc.UserMgmt.SetEmailConfirmedAsync(user, true);
                await _ioc.UserMgmt.SetPasswordConfirmedAsync(user, true);
                await _ioc.UserMgmt.SetPhoneNumberConfirmedAsync(user, true);
                await _ioc.UserMgmt.AddClaimAsync(user, new Claim(BaseLib.Statics.ApiUnitTestClaimType, BaseLib.Statics.ApiUnitTestClaimValue));

                if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                    await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

                if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                    await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));
            }
        }

        public async void Destroy()
        {
            foreach (var user in _ioc.UserMgmt.Store.Get())
                await _ioc.UserMgmt.DeleteAsync(user);

            foreach (var login in _ioc.LoginMgmt.Store.Get())
                await _ioc.LoginMgmt.DeleteAsync(login);

            foreach (var role in _ioc.RoleMgmt.Store.Get())
                await _ioc.RoleMgmt.DeleteAsync(role);

            foreach (var audience in _ioc.AudienceMgmt.Store.Get())
                await _ioc.AudienceMgmt.DeleteAsync(audience);

            foreach (var client in _ioc.ClientMgmt.Store.Get())
                await _ioc.ClientMgmt.DeleteAsync(client);
        }

        public async void DestroyDefaultData()
        {
            var user = await _ioc.UserMgmt.FindByNameAsync(Statics.ApiDefaultUserAdmin + "@local");

            if (user != null)
            {
                var roles = await _ioc.UserMgmt.GetRolesAsync(user);

                await _ioc.UserMgmt.RemoveFromRolesAsync(user, roles.ToArray());
                await _ioc.UserMgmt.DeleteAsync(user);
            }

            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (login != null)
                await _ioc.LoginMgmt.DeleteAsync(login);

            var roleAdmin = await _ioc.RoleMgmt.FindByNameAsync(Statics.ApiDefaultRoleForAdminUi);

            if (roleAdmin != null)
                await _ioc.RoleMgmt.DeleteAsync(roleAdmin);

            var roleViewer = await _ioc.RoleMgmt.FindByNameAsync(Statics.ApiDefaultRoleForViewerApi);

            if (roleViewer != null)
                await _ioc.RoleMgmt.DeleteAsync(roleViewer);

            var audienceApi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudienceApi).SingleOrDefault();

            if (audienceApi != null)
                await _ioc.AudienceMgmt.DeleteAsync(audienceApi);

            var audienceUi = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudienceUi).SingleOrDefault();

            if (audienceUi != null)
                await _ioc.AudienceMgmt.DeleteAsync(audienceUi);

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (client != null)
                await _ioc.ClientMgmt.DeleteAsync(client);
        }
    }
}
