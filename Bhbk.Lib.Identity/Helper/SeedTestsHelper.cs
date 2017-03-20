using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Security.Claims;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Helper
{
    public class SeedTestsHelper
    {
        private UnitOfWork _uow;

        public SeedTestsHelper(UnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
        }

        public async void CreateTestData()
        {
            for (int i = 0; i < 1; i++)
                _uow.RealmRepository.Create(new AppRealm
                {
                    Id = Guid.NewGuid(),
                    Name = "Realm-UnitTest-" + EntrophyHelper.GenerateRandomBase64(4),
                    Created = DateTime.Now,
                    Enabled = true,
                    Immutable = false
                });

            _uow.RealmRepository.Save();

            Guid realmID = _uow.RealmRepository.Get().First().Id;

            for (int i = 0; i < 1; i++)
                _uow.ClientRepository.Create(new AppClient
                {
                    Id = Guid.NewGuid(),
                    Name = "Client-UnitTest-" + EntrophyHelper.GenerateRandomBase64(4),
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
                    Name = "Audience-UnitTest-" + EntrophyHelper.GenerateRandomBase64(4),
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
                    Name = "Role-UnitTest-" + EntrophyHelper.GenerateRandomBase64(4),
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
                    Immutable = false,
                    RealmId = realmID
                }, BaseLib.Statics.ApiUnitTestsPassword);

            }

            foreach (var user in _uow.UserRepository.Get().ToList())
            {
                await _uow.CustomUserManager.AddClaimAsync(user.Id, new Claim("ClaimType-UnitTest", "ClaimValue-UnitTest-" + EntrophyHelper.GenerateRandomBase64(4)));

                foreach (var role in _uow.RoleRepository.Get().ToList())
                    await _uow.CustomUserManager.AddToRoleAsync(user.Id, role.Name);
            }
        }
    }
}
