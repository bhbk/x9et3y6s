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
        private IIdentityContext _ioc;

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
                    Enabled = true,
                    Immutable = false
                };

                await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(client).Devolve());
                foundClient = _ioc.ClientMgmt.Store.Get(x => x.Name == client.Name).Single();
            }

            var foundAudience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (foundAudience == null)
            {
                audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Statics.ApiDefaultAudience,
                    AudienceType = AudienceType.thin_client.ToString(),
                    AudienceKey = CryptoHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = false
                };

                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(audience).Devolve());
                foundAudience = _ioc.AudienceMgmt.Store.Get(x => x.Name == audience.Name).Single();
            }

            var foundUser = _ioc.UserMgmt.Store.Get(x => x.UserName == Statics.ApiDefaultUserAdmin).SingleOrDefault();

            if (foundUser == null)
            {
                user = new UserCreate()
                {
                    Email = Statics.ApiDefaultUserAdmin,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    LockoutEnabled = false,
                    Immutable = false
                };

                await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(user).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
                foundUser = _ioc.UserMgmt.Store.Get(x => x.Email == user.Email).Single();

                await _ioc.UserMgmt.SetEmailConfirmedAsync(foundUser, true);
                await _ioc.UserMgmt.SetPasswordConfirmedAsync(foundUser, true);
                await _ioc.UserMgmt.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdmin = _ioc.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForAdmin).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudience.Id,
                    Name = Statics.ApiDefaultRoleForAdmin,
                    Enabled = true,
                    Immutable = true
                };

                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(role).Devolve());
                foundRoleForAdmin = _ioc.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundRoleForViewer = _ioc.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForViewer).SingleOrDefault();

            if (foundRoleForViewer == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudience.Id,
                    Name = Statics.ApiDefaultRoleForViewer,
                    Enabled = true,
                    Immutable = true
                };

                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(role).Devolve());
                foundRoleForViewer = _ioc.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundLogin = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Statics.ApiDefaultLogin
                };

                await _ioc.LoginMgmt.CreateAsync(new LoginFactory<AppLogin>(login).Devolve());
                foundLogin = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == login.LoginProvider).SingleOrDefault();
            }

            if (!_ioc.UserMgmt.IsInLoginAsync(foundUser, Statics.ApiDefaultLogin).Result)
                await _ioc.UserMgmt.AddLoginAsync(foundUser,
                    new UserLoginInfo(Statics.ApiDefaultLogin, Statics.ApiDefaultLoginKey, Statics.ApiDefaultLoginName));

            if (!await _ioc.UserMgmt.IsInRoleAsync(foundUser, foundRoleForAdmin.Name))
                await _ioc.UserMgmt.AddToRoleAsync(foundUser, foundRoleForAdmin.Name);

            if (!await _ioc.UserMgmt.IsInRoleAsync(foundUser, foundRoleForViewer.Name))
                await _ioc.UserMgmt.AddToRoleAsync(foundUser, foundRoleForViewer.Name);
        }

        public async void CreateTestData()
        {
            AppAudience audience;
            AppClient client;

            for (int i = 0; i < 1; i++)
            {
                await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                    new ClientCreate()
                    {
                        Name = BaseLib.Statics.ApiUnitTestClient + CryptoHelper.GenerateRandomBase64(4),
                        Enabled = true,
                        Immutable = false
                    }).Devolve());
            }

            client = _ioc.ClientMgmt.Store.Get().First();

            for (int i = 0; i < 1; i++)
            {
                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                    new AudienceCreate()
                    {
                        ClientId = client.Id,
                        Name = BaseLib.Statics.ApiUnitTestAudience + CryptoHelper.GenerateRandomBase64(4),
                        AudienceType = AudienceType.thin_client.ToString(),
                        AudienceKey = CryptoHelper.GenerateRandomBase64(32),
                        Enabled = true,
                        Immutable = false
                    }).Devolve());
            }

            audience = _ioc.AudienceMgmt.Store.Get().First();

            for (int i = 0; i < 3; i++)
            {
                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(
                    new RoleCreate()
                    {
                        AudienceId = audience.Id,
                        Name = BaseLib.Statics.ApiUnitTestRole + CryptoHelper.GenerateRandomBase64(4),
                        Enabled = true,
                        Immutable = false
                    }).Devolve());
            }

            for (int i = 0; i < 1; i++)
            {
                string email = "unit-test@" + CryptoHelper.GenerateRandomBase64(4) + ".net";

                await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                    new UserCreate()
                    {
                        Email = email,
                        PhoneNumber = Statics.ApiDefaultPhone,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        LockoutEnabled = false,
                        Immutable = false
                    }).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);

                var user = _ioc.UserMgmt.Store.Get(x => x.Email == email).Single();

                await _ioc.UserMgmt.SetEmailConfirmedAsync(user, true);
                await _ioc.UserMgmt.SetPasswordConfirmedAsync(user, true);
                await _ioc.UserMgmt.SetPhoneNumberConfirmedAsync(user, true);
            }

            for (int i = 0; i < 1; i++)
            {
                await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                    new LoginCreate()
                    {
                        LoginProvider = BaseLib.Statics.ApiUnitTestLogin + CryptoHelper.GenerateRandomBase64(4)
                    }).Devolve());
            }

            var login = _ioc.LoginMgmt.Store.Get().First();

            foreach (var user in _ioc.UserMgmt.Store.Get())
            {
                await _ioc.UserMgmt.AddClaimAsync(user,
                    new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                        BaseLib.Statics.ApiUnitTestClaimValue + CryptoHelper.GenerateRandomBase64(4)));

                foreach (var role in _ioc.RoleMgmt.Store.Get())
                {
                    if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                        await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);
                }

                if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                {
                    await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "built-in"));
                }
            }
        }

        public async void Destroy()
        {
            foreach (var user in _ioc.UserMgmt.Store.Get())
                await _ioc.UserMgmt.DeleteAsync(user);

            foreach (var login in _ioc.LoginMgmt.Store.Get())
                await _ioc.LoginMgmt.DeleteAsync(login.Id);

            foreach (var role in _ioc.RoleMgmt.Store.Get())
                await _ioc.RoleMgmt.DeleteAsync(role);

            foreach (var audience in _ioc.AudienceMgmt.Store.Get())
                await _ioc.AudienceMgmt.DeleteAsync(audience.Id);

            foreach (var client in _ioc.ClientMgmt.Store.Get())
                await _ioc.ClientMgmt.DeleteAsync(client.Id);
        }

        public async void DestroyDefaultData()
        {
            var user = await _ioc.UserMgmt.FindByNameAsync(Statics.ApiDefaultUserAdmin);

            if (user != null)
            {
                var roles = await _ioc.UserMgmt.GetRolesAsync(user);

                await _ioc.UserMgmt.RemoveFromRolesAsync(user, roles.ToArray());
                await _ioc.UserMgmt.DeleteAsync(user);
            }

            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (login != null)
                await _ioc.LoginMgmt.DeleteAsync(login.Id);

            var role = await _ioc.RoleMgmt.FindByNameAsync(Statics.ApiDefaultRoleForAdmin);

            if (role != null)
                await _ioc.RoleMgmt.DeleteAsync(role);

            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (audience != null)
                await _ioc.AudienceMgmt.DeleteAsync(audience.Id);

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (client != null)
                await _ioc.ClientMgmt.DeleteAsync(client.Id);
        }
    }
}
