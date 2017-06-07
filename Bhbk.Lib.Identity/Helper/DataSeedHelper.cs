using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Security.Claims;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Helper
{
    public class DataSeedHelper
    {
        private UnitOfWork _uow;
        private AppClient _client;
        private AppAudience _audience;
        private AppProvider _provider;
        private AppRole _role;
        private AppUser _user;

        public DataSeedHelper(UnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
        }

        public async void CreateDefaultData()
        {
            var foundProvider = _uow.ProviderRepository.Get(x => x.Name == Statics.ApiDefaultProvider).SingleOrDefault();

            if (foundProvider == null)
            {
                _provider = new AppProvider()
                {
                    Id = Guid.NewGuid(),
                    Name = Statics.ApiDefaultProvider,
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                };

                await _uow.CustomProviderManager.CreateAsync(_provider);
                foundProvider = _uow.ProviderRepository.Get(x => x.Name == _provider.Name).Single();
            }

            var foundClient = _uow.ClientRepository.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (foundClient == null)
            {
                _client = new AppClient()
                {
                    Id = Guid.NewGuid(),
                    Name = Statics.ApiDefaultClient,
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                };

                _uow.ClientRepository.Create(_client);
                await _uow.ClientRepository.SaveAsync();
                foundClient = _uow.ClientRepository.Get(x => x.Name == Statics.ApiDefaultClient).Single();
            }

            var foundAudience = _uow.AudienceRepository.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (foundAudience == null)
            {
                _audience = new AppAudience()
                {
                    Id = Guid.NewGuid(),
                    ClientId = foundClient.Id,
                    Name = Statics.ApiDefaultAudience,
                    AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                    AudienceType = AudienceType.ThinClient.ToString(),
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                };

                _uow.AudienceRepository.Create(_audience);
                await _uow.AudienceRepository.SaveAsync();
                foundAudience = _uow.AudienceRepository.Get(x => x.Name == _audience.Name).Single();
            }

            var foundUser = _uow.UserRepository.Get(x => x.UserName == Statics.ApiDefaultAdmin).SingleOrDefault();

            if (foundUser == null)
            {
                _user = new AppUser()
                {
                    Id = Guid.NewGuid(),
                    UserName = Statics.ApiDefaultAdmin,
                    Email = Statics.ApiDefaultAdmin,
                    EmailConfirmed = true,
                    FirstName = "Uber",
                    LastName = "User",
                    PhoneNumber = "+0123456789",
                    PhoneNumberConfirmed = true,
                    Created = DateTime.Now,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    AccessSuccessCount = 0,
                    TwoFactorEnabled = false,
                    Immutable = false
                };

                await _uow.CustomUserManager.CreateAsync(_user, BaseLib.Statics.ApiUnitTestsPassword);
                foundUser = _uow.UserRepository.Get(x => x.UserName == _user.UserName).Single();
            }

            var foundRoleForAdmin = _uow.RoleRepository.Get(x => x.Name == Statics.ApiDefaultRoleForAdmin).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                _role = new AppRole()
                {
                    Id = Guid.NewGuid(),
                    Name = Statics.ApiDefaultRoleForAdmin,
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = true,
                    AudienceId = foundAudience.Id
                };

                await _uow.CustomRoleManager.CreateAsync(_role);
                foundRoleForAdmin = _uow.RoleRepository.Get(x => x.Name == _role.Name).Single();
            }

            var foundRoleForViewer = _uow.RoleRepository.Get(x => x.Name == Statics.ApiDefaultRoleForViewer).SingleOrDefault();

            if (foundRoleForViewer == null)
            {
                _role = new AppRole()
                {
                    Id = Guid.NewGuid(),
                    Name = Statics.ApiDefaultRoleForViewer,
                    Created = DateTime.Now,
                    Enabled = false,
                    Immutable = true,
                    AudienceId = foundAudience.Id
                };

                await _uow.CustomRoleManager.CreateAsync(_role);
                foundRoleForViewer = _uow.RoleRepository.Get(x => x.Name == _role.Name).Single();
            }

            if (!_uow.CustomUserManager.IsInProviderAsync(foundUser.Id, foundProvider.Name).Result)
                await _uow.CustomUserManager.AddToProviderAsync(foundUser.Id, foundProvider.Name);

            if (!_uow.CustomUserManager.IsInRoleAsync(foundUser.Id, foundRoleForAdmin.Name).Result)
                await _uow.CustomUserManager.AddToRoleAsync(foundUser.Id, foundRoleForAdmin.Name);

            if (!_uow.CustomUserManager.IsInRoleAsync(foundUser.Id, foundRoleForViewer.Name).Result)
                await _uow.CustomUserManager.AddToRoleAsync(foundUser.Id, foundRoleForViewer.Name);
        }

        public async void CreateTestData()
        {
            for (int i = 0; i < 1; i++)
                _uow.ProviderRepository.Create(new AppProvider
                {
                    Id = Guid.NewGuid(),
                    Name = BaseLib.Statics.ApiUnitTestsProvider + EntrophyHelper.GenerateRandomBase64(4),
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                });

            _uow.ProviderRepository.Save();

            Guid providerID = _uow.ProviderRepository.Get().First().Id;

            for (int i = 0; i < 1; i++)
                _uow.ClientRepository.Create(new AppClient
                {
                    Id = Guid.NewGuid(),
                    Name = BaseLib.Statics.ApiUnitTestsClient + EntrophyHelper.GenerateRandomBase64(4),
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                });

            _uow.ClientRepository.Save();

            Guid clientID = _uow.ClientRepository.Get().First().Id;

            for (int i = 0; i < 1; i++)
                _uow.AudienceRepository.Create(new AppAudience
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientID,
                    Name = BaseLib.Statics.ApiUnitTestsAudience + EntrophyHelper.GenerateRandomBase64(4),
                    AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                    AudienceType = AudienceType.ThinClient.ToString(),
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                });

            _uow.AudienceRepository.Save();

            Guid audienceID = _uow.AudienceRepository.Get().First().Id;

            for (int i = 0; i < 3; i++)
                await _uow.CustomRoleManager.CreateAsync(new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = BaseLib.Statics.ApiUnitTestsRole + EntrophyHelper.GenerateRandomBase64(4),
                    Enabled = true,
                    Immutable = false,
                    AudienceId = audienceID
                });

            for (int i = 0; i < 3; i++)
            {
                string email = "unit-test@" + EntrophyHelper.GenerateRandomBase64(4) + ".net";

                await _uow.CustomUserManager.CreateAsync(new AppUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    LockoutEnabled = false,
                    LockoutEndDateUtc = DateTime.Now.AddDays(30),
                    TwoFactorEnabled = false,
                    Immutable = false
                }, BaseLib.Statics.ApiUnitTestsPassword);

            }

            foreach (var user in _uow.UserRepository.Get().ToList())
            {
                await _uow.CustomUserManager.AddToProviderAsync(user.Id, _uow.ProviderRepository.Get().First().Name);

                await _uow.CustomUserManager.AddClaimAsync(user.Id,
                    new Claim(BaseLib.Statics.ApiUnitTestsClaimType,
                        BaseLib.Statics.ApiUnitTestsClaimValue + EntrophyHelper.GenerateRandomBase64(4)));

                foreach (var role in _uow.RoleRepository.Get().ToList())
                    await _uow.CustomUserManager.AddToRoleAsync(user.Id, role.Name);
            }
        }

        public async void DestroyDefaultData()
        {
            foreach (var user in _uow.UserRepository.Get())
                await _uow.CustomUserManager.DeleteAsync(user);

            foreach (var role in _uow.RoleRepository.Get())
                await _uow.CustomRoleManager.DeleteAsync(role);

            foreach (var audience in _uow.AudienceRepository.Get())
                _uow.AudienceRepository.Delete(audience);

            await _uow.AudienceRepository.SaveAsync();

            foreach (var provider in _uow.ProviderRepository.Get())
                _uow.ProviderRepository.Delete(provider);

            await _uow.ProviderRepository.SaveAsync();

            foreach (var client in _uow.ClientRepository.Get())
                _uow.ClientRepository.Delete(client);

            await _uow.ClientRepository.SaveAsync();
        }

        public async void DestroyAllData()
        {
            var user = await _uow.CustomUserManager.FindByNameAsync(Statics.ApiDefaultAdmin);

            if (user != null)
            {
                var userRoles = await _uow.CustomUserManager.GetRolesAsync(user.Id);

                await _uow.CustomUserManager.RemoveFromRolesAsync(user.Id, userRoles.ToArray());
                await _uow.CustomUserManager.DeleteAsync(user);
            }

            var role = await _uow.CustomRoleManager.FindByNameAsync(Statics.ApiDefaultRoleForAdmin);

            if (role != null)
                await _uow.CustomRoleManager.DeleteAsync(role);

            var audience = _uow.AudienceRepository.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (audience != null)
            {
                _uow.AudienceRepository.Delete(audience);
                await _uow.AudienceRepository.SaveAsync();
            }

            var provider = _uow.ProviderRepository.Get(x => x.Name == Statics.ApiDefaultProvider).SingleOrDefault();

            if (provider != null)
            {
                _uow.ProviderRepository.Delete(provider);
                await _uow.ProviderRepository.SaveAsync();
            }

            var client = _uow.ClientRepository.Get(x => x.Name == Statics.ApiDefaultClient).SingleOrDefault();

            if (client != null)
            {
                _uow.ClientRepository.Delete(client);
                await _uow.ClientRepository.SaveAsync();
            }
        }
    }
}
