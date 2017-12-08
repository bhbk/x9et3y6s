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
    public class DataHelper
    {
        private IIdentityContext _context;

        public DataHelper(IIdentityContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _context = context;
        }

        public async void CompleteDestroy()
        {
            foreach (var user in _context.UserMgmt.Store.Get())
                await _context.UserMgmt.DeleteAsync(user);

            foreach (var login in _context.LoginMgmt.Store.Get())
                await _context.LoginMgmt.DeleteAsync(login.Id);

            foreach (var role in _context.RoleMgmt.Store.Get())
                await _context.RoleMgmt.DeleteAsync(role);

            foreach (var audience in _context.AudienceMgmt.Store.Get())
                await _context.AudienceMgmt.DeleteAsync(audience.Id);

            foreach (var client in _context.ClientMgmt.Store.Get())
                await _context.ClientMgmt.DeleteAsync(client.Id);
        }

        public async void DefaultDataCreate()
        {
            AudienceCreate audience;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            var foundClient = _context.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (foundClient == null)
            {
                client = new ClientCreate()
                {
                    Name = Statics.ApiDefaultClient,
                    Enabled = true,
                    Immutable = false
                };

                await _context.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(client).Devolve());
                foundClient = _context.ClientMgmt.Store.Get(x => x.Name == client.Name).Single();
            }

            var foundAudience = _context.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (foundAudience == null)
            {
                audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Statics.ApiDefaultAudience,
                    AudienceType = AudienceType.thin_client.ToString(),
                    AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = false
                };

                await _context.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(audience).Devolve());
                foundAudience = _context.AudienceMgmt.Store.Get(x => x.Name == audience.Name).Single();
            }

            var foundUser = _context.UserMgmt.Store.Get(x => x.UserName == Statics.ApiDefaultUserAdmin).SingleOrDefault();

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

                await _context.UserMgmt.CreateAsync(new UserFactory<UserCreate>(user).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
                foundUser = _context.UserMgmt.Store.Get(x => x.Email == user.Email).Single();

                await _context.UserMgmt.SetEmailConfirmedAsync(foundUser, true);
                await _context.UserMgmt.SetPasswordConfirmedAsync(foundUser, true);
                await _context.UserMgmt.SetPhoneNumberConfirmedAsync(foundUser, true);
            }

            var foundRoleForAdmin = _context.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForAdmin).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudience.Id,
                    Name = Statics.ApiDefaultRoleForAdmin,
                    Enabled = true,
                    Immutable = true
                };

                await _context.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(role).Devolve());
                foundRoleForAdmin = _context.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundRoleForViewer = _context.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForViewer).SingleOrDefault();

            if (foundRoleForViewer == null)
            {
                role = new RoleCreate()
                {
                    AudienceId = foundAudience.Id,
                    Name = Statics.ApiDefaultRoleForViewer,
                    Enabled = true,
                    Immutable = true
                };

                await _context.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(role).Devolve());
                foundRoleForViewer = _context.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundLogin = _context.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Statics.ApiDefaultLogin
                };

                await _context.LoginMgmt.CreateAsync(new LoginFactory<AppLogin>(login).Devolve());
                foundLogin = _context.LoginMgmt.Store.Get(x => x.LoginProvider == login.LoginProvider).SingleOrDefault();
            }

            if (!_context.UserMgmt.IsInLoginAsync(foundUser, Statics.ApiDefaultLogin).Result)
                await _context.UserMgmt.AddLoginAsync(foundUser, 
                    new UserLoginInfo(Statics.ApiDefaultLogin, Statics.ApiDefaultLoginKey, Statics.ApiDefaultLoginName));

            if (!await _context.UserMgmt.IsInRoleAsync(foundUser, foundRoleForAdmin.Name))
                await _context.UserMgmt.AddToRoleAsync(foundUser, foundRoleForAdmin.Name);

            if (!await _context.UserMgmt.IsInRoleAsync(foundUser, foundRoleForViewer.Name))
                await _context.UserMgmt.AddToRoleAsync(foundUser, foundRoleForViewer.Name);
        }

        public async void DefaultDataDestroy()
        {
            var user = await _context.UserMgmt.FindByNameAsync(Statics.ApiDefaultUserAdmin);

            if (user != null)
            {
                var roles = await _context.UserMgmt.GetRolesAsync(user);

                await _context.UserMgmt.RemoveFromRolesAsync(user, roles.ToArray());
                await _context.UserMgmt.DeleteAsync(user);
            }

            var login = _context.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (login != null)
                await _context.LoginMgmt.DeleteAsync(login.Id);

            var role = await _context.RoleMgmt.FindByNameAsync(Statics.ApiDefaultRoleForAdmin);

            if (role != null)
                await _context.RoleMgmt.DeleteAsync(role);

            var audience = _context.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (audience != null)
                await _context.AudienceMgmt.DeleteAsync(audience.Id);

            var client = _context.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (client != null)
                await _context.ClientMgmt.DeleteAsync(client.Id);
        }

        public async void TestDataCreate()
        {
            AppAudience audience;
            AppClient client;

            for (int i = 0; i < 1; i++)
            {
                await _context.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                    new ClientCreate()
                    {
                        Name = BaseLib.Statics.ApiUnitTestClient + EntrophyHelper.GenerateRandomBase64(4),
                        Enabled = true,
                        Immutable = false
                    }).Devolve());
            }

            client = _context.ClientMgmt.Store.Get().First();

            for (int i = 0; i < 1; i++)
            {
                await _context.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                    new AudienceCreate()
                    {
                        ClientId = client.Id,
                        Name = BaseLib.Statics.ApiUnitTestAudience + EntrophyHelper.GenerateRandomBase64(4),
                        AudienceType = AudienceType.thin_client.ToString(),
                        AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                        Enabled = true,
                        Immutable = false
                    }).Devolve());
            }

            audience = _context.AudienceMgmt.Store.Get().First();

            for (int i = 0; i < 3; i++)
            {
                await _context.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(
                    new RoleCreate()
                    {
                        AudienceId = audience.Id,
                        Name = BaseLib.Statics.ApiUnitTestRole + EntrophyHelper.GenerateRandomBase64(4),
                        Enabled = true,
                        Immutable = false
                    }).Devolve());
            }

            for (int i = 0; i < 1; i++)
            {
                string email = "unit-test@" + EntrophyHelper.GenerateRandomBase64(4) + ".net";

                await _context.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                    new UserCreate()
                    {
                        Email = email,
                        PhoneNumber = Statics.ApiDefaultPhone,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        LockoutEnabled = false,
                        Immutable = false
                    }).Devolve(), BaseLib.Statics.ApiUnitTestPasswordCurrent);

                var user = _context.UserMgmt.Store.Get(x => x.Email == email).Single();

                await _context.UserMgmt.SetEmailConfirmedAsync(user, true);
                await _context.UserMgmt.SetPasswordConfirmedAsync(user, true);
                await _context.UserMgmt.SetPhoneNumberConfirmedAsync(user, true);
            }

            for (int i = 0; i < 1; i++)
            {
                await _context.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                    new LoginCreate()
                    {
                        LoginProvider = BaseLib.Statics.ApiUnitTestLogin + EntrophyHelper.GenerateRandomBase64(4)
                    }).Devolve());
            }

            var login = _context.LoginMgmt.Store.Get().First();

            foreach (var user in _context.UserMgmt.Store.Get())
            {
                await _context.UserMgmt.AddClaimAsync(user,
                    new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                        BaseLib.Statics.ApiUnitTestClaimValue + EntrophyHelper.GenerateRandomBase64(4)));

                foreach (var role in _context.RoleMgmt.Store.Get())
                {
                    if (!await _context.UserMgmt.IsInRoleAsync(user, role.Name))
                        await _context.UserMgmt.AddToRoleAsync(user, role.Name);
                }

                if (!await _context.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                {
                    await _context.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "built-in"));
                }
            }
        }
    }
}
