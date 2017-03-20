using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Helper
{
    public class SeedDefaultsHelper
    {
        private CustomIdentityDbContext _context;
        private UnitOfWork _uow;
        private AppClient _client;
        private AppAudience _audience;
        private AppRealm _realm;
        private AppRole _role;
        private AppUser _user;

        public SeedDefaultsHelper(CustomIdentityDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            this._context = context;
            this._uow = new UnitOfWork(_context);
        }

        public async void CreateDefaultData()
        {
            var foundRealm = _uow.RealmRepository.Get(x => x.Name == Statics.ApiDefaultRealm).SingleOrDefault();

            if (foundRealm == null)
            {
                _realm = new AppRealm()
                {
                    Id = Guid.NewGuid(),
                    Name = Statics.ApiDefaultRealm,
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                };

                _uow.RealmRepository.Create(_realm);
                await _uow.RealmRepository.SaveAsync();
                foundRealm = _uow.RealmRepository.Get(x => x.Name == _realm.Name).Single();
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
                    Immutable = false,
                    RealmId = foundRealm.Id
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

            if (!_uow.CustomUserManager.IsInRoleAsync(foundUser.Id, foundRoleForAdmin.Name).Result)
                await _uow.CustomUserManager.AddToRoleAsync(foundUser.Id, foundRoleForAdmin.Name);

            if (!_uow.CustomUserManager.IsInRoleAsync(foundUser.Id, foundRoleForViewer.Name).Result)
                await _uow.CustomUserManager.AddToRoleAsync(foundUser.Id, foundRoleForViewer.Name);
        }

        public async void DestroyDefaultData()
        {
            foreach (var user in _uow.UserRepository.Get())
                await _uow.CustomUserManager.DeleteAsync(user);

            foreach (var realm in _uow.RealmRepository.Get())
                _uow.RealmRepository.Delete(realm);

            await _uow.RealmRepository.SaveAsync();

            foreach (var audience in _uow.AudienceRepository.Get())
                _uow.AudienceRepository.Delete(audience);

            await _uow.AudienceRepository.SaveAsync();

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

            var realm = _uow.RealmRepository.Get(x => x.Name == Statics.ApiDefaultRealm).SingleOrDefault();

            if (realm != null)
            {
                _uow.RealmRepository.Delete(realm);
                await _uow.RealmRepository.SaveAsync();
            }

            var audience = _uow.AudienceRepository.Get(x => x.Name == Statics.ApiDefaultAudience).SingleOrDefault();

            if (audience != null)
            {
                _uow.AudienceRepository.Delete(audience);
                await _uow.AudienceRepository.SaveAsync();
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
