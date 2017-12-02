using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
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

                await _context.ClientMgmt.CreateAsync(_context.ClientMgmt.Store.Mf.Create.DoIt(client));
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

                await _context.AudienceMgmt.CreateAsync(_context.AudienceMgmt.Store.Mf.Create.DoIt(audience));
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

                await _context.UserMgmt.CreateAsync(_context.UserMgmt.Store.Mf.Create.DoIt(user), BaseLib.Statics.ApiUnitTestPasswordCurrent);
                foundUser = _context.UserMgmt.Store.Get(x => x.Email == user.Email).Single();

                await _context.UserMgmt.SetEmailConfirmedAsync(foundUser.Id, true);
                await _context.UserMgmt.SetPasswordConfirmedAsync(foundUser.Id, true);
                await _context.UserMgmt.SetPhoneNumberConfirmedAsync(foundUser.Id, true);
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

                await _context.RoleMgmt.CreateAsync(_context.RoleMgmt.Store.Mf.Create.DoIt(role));
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

                await _context.RoleMgmt.CreateAsync(_context.RoleMgmt.Store.Mf.Create.DoIt(role));
                foundRoleForViewer = _context.RoleMgmt.Store.Get(x => x.Name == role.Name).Single();
            }

            var foundLogin = _context.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Statics.ApiDefaultLogin
                };

                await _context.LoginMgmt.CreateAsync(_context.LoginMgmt.Store.Mf.Create.DoIt(login));
                foundLogin = _context.LoginMgmt.Store.Get(x => x.LoginProvider == login.LoginProvider).SingleOrDefault();
            }

            if (!_context.UserMgmt.IsInLoginAsync(foundUser.Id, Statics.ApiDefaultLogin).Result)
                await _context.UserMgmt.AddLoginAsync(foundUser.Id, _context.UserMgmt.Store.Mf.Create.DoIt(
                    new UserLoginCreate()
                    {
                        UserId = foundUser.Id,
                        LoginId = foundLogin.Id,
                        LoginProvider = Statics.ApiDefaultLogin,
                        ProviderDisplayName = Statics.ApiDefaultLogin,
                        ProviderDescription = "built-in",
                        ProviderKey = "built-in",
                        Enabled = true,
                        Immutable = false
                    }));

            if (!await _context.UserMgmt.IsInRoleAsync(foundUser.Id, foundRoleForAdmin.Name))
                await _context.UserMgmt.AddToRoleAsync(foundUser.Id, foundRoleForAdmin.Name);

            if (!await _context.UserMgmt.IsInRoleAsync(foundUser.Id, foundRoleForViewer.Name))
                await _context.UserMgmt.AddToRoleAsync(foundUser.Id, foundRoleForViewer.Name);
        }

        public async void DefaultDataDestroy()
        {
            var user = await _context.UserMgmt.FindByNameAsync(Statics.ApiDefaultUserAdmin);

            if (user != null)
            {
                var roles = await _context.UserMgmt.GetRolesAsync(user.Id);

                await _context.UserMgmt.RemoveFromRolesAsync(user.Id, roles.ToArray());
                await _context.UserMgmt.DeleteAsync(user.Id);
            }

            var login = _context.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiDefaultLogin).SingleOrDefault();

            if (login != null)
                await _context.LoginMgmt.DeleteAsync(login.Id);

            var role = await _context.RoleMgmt.FindByNameAsync(Statics.ApiDefaultRoleForAdmin);

            if (role != null)
                await _context.RoleMgmt.DeleteAsync(role.Id);

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
                await _context.ClientMgmt.CreateAsync(_context.ClientMgmt.Store.Mf.Create.DoIt(
                    new ClientCreate()
                    {
                        Name = BaseLib.Statics.ApiUnitTestClient + EntrophyHelper.GenerateRandomBase64(4),
                        Enabled = true,
                        Immutable = false
                    }));
            }

            client = _context.ClientMgmt.Store.Get().First();

            for (int i = 0; i < 1; i++)
            {
                await _context.AudienceMgmt.CreateAsync(_context.AudienceMgmt.Store.Mf.Create.DoIt(
                    new AudienceCreate()
                    {
                        ClientId = client.Id,
                        Name = BaseLib.Statics.ApiUnitTestAudience + EntrophyHelper.GenerateRandomBase64(4),
                        AudienceType = AudienceType.thin_client.ToString(),
                        AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                        Enabled = true,
                        Immutable = false
                    }));
            }

            audience = _context.AudienceMgmt.Store.Get().First();

            for (int i = 0; i < 3; i++)
            {
                await _context.RoleMgmt.CreateAsync(_context.RoleMgmt.Store.Mf.Create.DoIt(
                    new RoleCreate()
                    {
                        AudienceId = audience.Id,
                        Name = BaseLib.Statics.ApiUnitTestRole + EntrophyHelper.GenerateRandomBase64(4),
                        Enabled = true,
                        Immutable = false
                    }));
            }

            for (int i = 0; i < 1; i++)
            {
                string email = "unit-test@" + EntrophyHelper.GenerateRandomBase64(4) + ".net";

                await _context.UserMgmt.CreateAsync(_context.UserMgmt.Store.Mf.Create.DoIt(
                    new UserCreate()
                    {
                        Email = email,
                        PhoneNumber = Statics.ApiDefaultPhone,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        LockoutEnabled = false,
                        Immutable = false
                    }), BaseLib.Statics.ApiUnitTestPasswordCurrent);

                var user = _context.UserMgmt.Store.Get(x => x.Email == email).Single();

                await _context.UserMgmt.SetEmailConfirmedAsync(user.Id, true);
                await _context.UserMgmt.SetPasswordConfirmedAsync(user.Id, true);
                await _context.UserMgmt.SetPhoneNumberConfirmedAsync(user.Id, true);
            }

            for (int i = 0; i < 1; i++)
            {
                await _context.LoginMgmt.CreateAsync(_context.LoginMgmt.Store.Mf.Create.DoIt(
                    new LoginCreate()
                    {
                        LoginProvider = BaseLib.Statics.ApiUnitTestLogin + EntrophyHelper.GenerateRandomBase64(4)
                    }));
            }

            var login = _context.LoginMgmt.Store.Get().First();

            foreach (var user in _context.UserMgmt.Store.Get())
            {
                await _context.UserMgmt.AddClaimAsync(user.Id,
                    new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                        BaseLib.Statics.ApiUnitTestClaimValue + EntrophyHelper.GenerateRandomBase64(4)));

                foreach (var role in _context.RoleMgmt.Store.Get())
                {
                    if (!await _context.UserMgmt.IsInRoleAsync(user, role.Name))
                        await _context.UserMgmt.AddToRoleAsync(user.Id, role.Name);
                }

                if (!await _context.UserMgmt.IsInLoginAsync(user.Id, login.LoginProvider))
                {
                    await _context.UserMgmt.AddLoginAsync(user.Id, _context.UserMgmt.Store.Mf.Create.DoIt(
                        new UserLoginCreate()
                        {
                            UserId = user.Id,
                            LoginId = login.Id,
                            LoginProvider = login.LoginProvider,
                            ProviderDisplayName = login.LoginProvider,
                            ProviderDescription = "unit-test",
                            ProviderKey = "unit-test",
                            Enabled = true,
                            Immutable = false
                        }));
                }
            }
        }
    }
}
