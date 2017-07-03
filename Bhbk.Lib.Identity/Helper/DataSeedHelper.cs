using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using System;
using System.Linq;
using System.Security.Claims;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Helper
{
    public class DataSeedHelper
    {
        private UnitOfWork _uow;
        private ClientCreate _client;
        private AudienceCreate _audience;
        private ProviderCreate _provider;
        private RoleCreate _role;
        private UserCreate _user;

        public DataSeedHelper(UnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
        }

        public async void CreateDefaultData()
        {
            var foundProvider = _uow.ProviderMgmt.Store.Get(x => x.Name == Statics.ApiDefaultProvider).SingleOrDefault();

            if (foundProvider == null)
            {
                _provider = new ProviderCreate()
                {
                    Name = Statics.ApiDefaultProvider,
                    Enabled = true,
                    Immutable = false
                };

                await _uow.ProviderMgmt.CreateAsync(_uow.ProviderMgmt.Store.Mf.Create.DoIt(_provider));
                foundProvider = _uow.ProviderMgmt.Store.Get(x => x.Name == _provider.Name).Single();
            }

            var foundClient = _uow.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (foundClient == null)
            {
                _client = new ClientCreate()
                {
                    Name = Statics.ApiDefaultClient,
                    Enabled = true,
                    Immutable = false
                };

                await _uow.ClientMgmt.CreateAsync(_uow.ClientMgmt.Store.Mf.Create.DoIt(_client));
                foundClient = _uow.ClientMgmt.Store.Get(x => x.Name == _client.Name).Single();
            }

            var foundAudience = _uow.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (foundAudience == null)
            {
                _audience = new AudienceCreate()
                {
                    ClientId = foundClient.Id,
                    Name = Statics.ApiDefaultAudience,
                    AudienceType = AudienceType.ThinClient.ToString(),
                    AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = false
                };

                await _uow.AudienceMgmt.CreateAsync(_uow.AudienceMgmt.Store.Mf.Create.DoIt(_audience));
                foundAudience = _uow.AudienceMgmt.Store.Get(x => x.Name == _audience.Name).Single();
            }

            var foundUser = _uow.UserMgmt.Store.Get(x => x.UserName == Statics.ApiDefaultAdmin).SingleOrDefault();

            if (foundUser == null)
            {
                _user = new UserCreate()
                {
                    Email = Statics.ApiDefaultAdmin,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    LockoutEnabled = false,
                    Immutable = false
                };

                await _uow.UserMgmt.CreateAsync(_uow.UserMgmt.Store.Mf.Create.DoIt(_user), BaseLib.Statics.ApiUnitTestPasswordCurrent);
                foundUser = _uow.UserMgmt.Store.Get(x => x.Email == _user.Email).Single();

                await _uow.UserMgmt.SetEmailConfirmedAsync(foundUser.Id, true);
                await _uow.UserMgmt.SetPasswordConfirmedAsync(foundUser.Id, true);
                await _uow.UserMgmt.SetPhoneNumberConfirmedAsync(foundUser.Id, true);
            }

            var foundRoleForAdmin = _uow.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForAdmin).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                _role = new RoleCreate()
                {
                    AudienceId = foundAudience.Id,
                    Name = Statics.ApiDefaultRoleForAdmin,
                    Enabled = true,
                    Immutable = true
                };

                await _uow.RoleMgmt.CreateAsync(_uow.RoleMgmt.Store.Mf.Create.DoIt(_role));
                foundRoleForAdmin = _uow.RoleMgmt.Store.Get(x => x.Name == _role.Name).SingleOrDefault();
            }

            var foundRoleForViewer = _uow.RoleMgmt.Store.Get(x => x.Name == Statics.ApiDefaultRoleForViewer).SingleOrDefault();

            if (foundRoleForViewer == null)
            {
                _role = new RoleCreate()
                {
                    AudienceId = foundAudience.Id,
                    Name = Statics.ApiDefaultRoleForViewer,
                    Enabled = true,
                    Immutable = true
                };

                await _uow.RoleMgmt.CreateAsync(_uow.RoleMgmt.Store.Mf.Create.DoIt(_role));
                foundRoleForViewer = _uow.RoleMgmt.Store.Get(x => x.Name == _role.Name).SingleOrDefault();
            }

            if (!_uow.UserMgmt.IsInProviderAsync(foundUser.Id, foundProvider.Name).Result)
                await _uow.UserMgmt.AddToProviderAsync(foundUser.Id, foundProvider.Name);

            if (!_uow.UserMgmt.IsInRoleAsync(foundUser.Id, foundRoleForAdmin.Name).Result)
                await _uow.UserMgmt.AddToRoleAsync(foundUser.Id, foundRoleForAdmin.Name);

            if (!_uow.UserMgmt.IsInRoleAsync(foundUser.Id, foundRoleForViewer.Name).Result)
                await _uow.UserMgmt.AddToRoleAsync(foundUser.Id, foundRoleForViewer.Name);
        }

        public async void CreateTestData()
        {
            for (int i = 0; i < 1; i++)
                await _uow.ProviderMgmt.CreateAsync(_uow.ProviderMgmt.Store.Mf.Create.DoIt(new ProviderCreate()
                {
                    Name = BaseLib.Statics.ApiUnitTestProvider + EntrophyHelper.GenerateRandomBase64(4),
                    Enabled = true,
                    Immutable = false
                }));

            Guid providerID = _uow.ProviderMgmt.Store.Get().First().Id;

            for (int i = 0; i < 1; i++)
                await _uow.ClientMgmt.CreateAsync(_uow.ClientMgmt.Store.Mf.Create.DoIt(new ClientCreate()
                {
                    Name = BaseLib.Statics.ApiUnitTestClient + EntrophyHelper.GenerateRandomBase64(4),
                    Enabled = true,
                    Immutable = false
                }));

            Guid clientID = _uow.ClientMgmt.Store.Get().First().Id;

            for (int i = 0; i < 1; i++)
                await _uow.AudienceMgmt.CreateAsync(_uow.AudienceMgmt.Store.Mf.Create.DoIt(new AudienceCreate()
                {
                    ClientId = clientID,
                    Name = BaseLib.Statics.ApiUnitTestAudience + EntrophyHelper.GenerateRandomBase64(4),
                    AudienceType = AudienceType.ThinClient.ToString(),
                    AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                    Enabled = true,
                    Immutable = false
                }));

            Guid audienceID = _uow.AudienceMgmt.Store.Get().First().Id;

            for (int i = 0; i < 3; i++)
                await _uow.RoleMgmt.CreateAsync(_uow.RoleMgmt.Store.Mf.Create.DoIt(new RoleCreate()
                {
                    AudienceId = audienceID,
                    Name = BaseLib.Statics.ApiUnitTestRole + EntrophyHelper.GenerateRandomBase64(4),
                    Enabled = true,
                    Immutable = false
                }));

            for (int i = 0; i < 3; i++)
            {
                string email = "unit-test@" + EntrophyHelper.GenerateRandomBase64(4) + ".net";

                await _uow.UserMgmt.CreateAsync(_uow.UserMgmt.Store.Mf.Create.DoIt(new UserCreate()
                {
                    Email = email,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    LockoutEnabled = false,
                    Immutable = false
                }), BaseLib.Statics.ApiUnitTestPasswordCurrent);

                var user = _uow.UserMgmt.Store.Get(x => x.Email == email).Single();

                await _uow.UserMgmt.SetEmailConfirmedAsync(user.Id, true);
                await _uow.UserMgmt.SetPasswordConfirmedAsync(user.Id, true);
                await _uow.UserMgmt.SetPhoneNumberConfirmedAsync(user.Id, true);
            }

            foreach (var user in _uow.UserMgmt.Store.Get().ToList())
            {
                await _uow.UserMgmt.AddToProviderAsync(user.Id, _uow.ProviderMgmt.Store.Get().First().Name);

                await _uow.UserMgmt.AddClaimAsync(user.Id,
                    new Claim(BaseLib.Statics.ApiUnitTestClaimType,
                        BaseLib.Statics.ApiUnitTestClaimValue + EntrophyHelper.GenerateRandomBase64(4)));

                foreach (var role in _uow.RoleMgmt.Store.Get().ToList())
                    await _uow.UserMgmt.AddToRoleAsync(user.Id, role.Name);
            }
        }

        public async void DestroyDefaultData()
        {
            foreach (var user in _uow.UserMgmt.Store.Get())
                await _uow.UserMgmt.DeleteAsync(user);

            foreach (var role in _uow.RoleMgmt.Store.Get())
                await _uow.RoleMgmt.DeleteAsync(role);

            foreach (var audience in _uow.AudienceMgmt.Store.Get())
                await _uow.AudienceMgmt.DeleteAsync(audience.Id);

            foreach (var provider in _uow.ProviderMgmt.Store.Get())
                await _uow.ProviderMgmt.DeleteAsync(provider.Id);

            foreach (var client in _uow.ClientMgmt.Store.Get())
                await _uow.ClientMgmt.DeleteAsync(client.Id);
        }

        public async void DestroyAllData()
        {
            var user = await _uow.UserMgmt.FindByNameAsyncDeprecated(Statics.ApiDefaultAdmin);

            if (user != null)
            {
                var userRoles = await _uow.UserMgmt.GetRolesAsync(user.Id);

                await _uow.UserMgmt.RemoveFromRolesAsync(user.Id, userRoles.ToArray());
                await _uow.UserMgmt.DeleteAsync(user);
            }

            var role = await _uow.RoleMgmt.FindByNameAsync(Statics.ApiDefaultRoleForAdmin);

            if (role != null)
                await _uow.RoleMgmt.DeleteAsync(role.Id);

            var audience = _uow.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (audience != null)
                await _uow.AudienceMgmt.DeleteAsync(audience.Id);

            var provider = _uow.ProviderMgmt.Store.Get(x => x.Name == Statics.ApiDefaultProvider).SingleOrDefault();

            if (provider != null)
                await _uow.ProviderMgmt.DeleteAsync(provider.Id);

            var client = _uow.ClientMgmt.Store.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (client != null)
                await _uow.ClientMgmt.DeleteAsync(client.Id);
        }
    }
}
