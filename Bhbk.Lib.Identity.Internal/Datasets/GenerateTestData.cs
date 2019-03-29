using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Datasets
{
    public class GenerateTestData
    {
        private readonly IIdentityContext<AppDbContext> _uow;

        public GenerateTestData(IIdentityContext<AppDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async Task CreateAsync()
        {
            if (_uow.Situation != ExecutionType.UnitTest)
                throw new InvalidOperationException();

            AppIssuer issuer1, issuer2;
            AppClient client1, client2;
            AppClientUri uri1, uri2;
            AppRole role1, role2;
            AppLogin login1, login2;
            AppUser user1, user2;
            AppClaim claim1, claim2;

            //create test issuers
            issuer1 = await _uow.IssuerRepo.CreateAsync(new IssuerCreate()
            {
                Name = Strings.ApiUnitTestIssuer1,
                IssuerKey = Strings.ApiUnitTestIssuer1Key,
                Enabled = true,
                Immutable = false,
            });

            issuer2 = await _uow.IssuerRepo.CreateAsync(new IssuerCreate()
            {
                Name = Strings.ApiUnitTestIssuer2,
                IssuerKey = Strings.ApiUnitTestIssuer2Key,
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test clients
            client1 = await _uow.ClientRepo.CreateAsync(new ClientCreate()
            {
                IssuerId = issuer1.Id,
                Name = Strings.ApiUnitTestClient1,
                ClientKey = Strings.ApiUnitTestClient1Key,
                ClientType = ClientType.user_agent.ToString(),
                Enabled = true,
                Immutable = false,
            });

            client2 = await _uow.ClientRepo.CreateAsync(new ClientCreate()
            {
                IssuerId = issuer2.Id,
                Name = Strings.ApiUnitTestClient2,
                ClientKey = Strings.ApiUnitTestClient2Key,
                ClientType = ClientType.server.ToString(),
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //assign test client uris
            uri1 = await _uow.ClientRepo.CreateUriAsync(new ClientUriCreate()
            {
                ClientId = client1.Id,
                AbsoluteUri = Strings.ApiUnitTestUri1Link,
                Enabled = true,
            });

            uri2 = await _uow.ClientRepo.CreateUriAsync(new ClientUriCreate()
            {
                ClientId = client2.Id,
                AbsoluteUri = Strings.ApiUnitTestUri2Link,
                Enabled = true,
            });

            await _uow.CommitAsync();

            //create test claims
            claim1 = await _uow.ClaimRepo.CreateAsync(new ClaimCreate()
            {
                IssuerId = issuer1.Id,
                Type = Strings.ApiUnitTestClaim1,
                Value = RandomValues.CreateBase64String(8),
                Immutable = false,
            });

            claim2 = await _uow.ClaimRepo.CreateAsync(new ClaimCreate()
            {
                IssuerId = issuer2.Id,
                Type = Strings.ApiUnitTestClaim2,
                Value = RandomValues.CreateBase64String(8),
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test logins
            login1 = await _uow.LoginRepo.CreateAsync(new LoginCreate()
            {
                Name = Strings.ApiUnitTestLogin1,
                Immutable = false,
            });

            login2 = await _uow.LoginRepo.CreateAsync(new LoginCreate()
            {
                Name = Strings.ApiUnitTestLogin2,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test roles
            role1 = await _uow.RoleRepo.CreateAsync(new RoleCreate()
            {
                ClientId = client1.Id,
                Name = Strings.ApiUnitTestRole1,
                Enabled = true,
                Immutable = false,
            });

            role2 = await _uow.RoleRepo.CreateAsync(new RoleCreate()
            {
                ClientId = client2.Id,
                Name = Strings.ApiUnitTestRole2,
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test user 1
            user1 = await _uow.UserRepo.CreateAsync(new UserCreate()
            {
                Email = Strings.ApiUnitTestUser1,
                PhoneNumber = Strings.ApiDefaultPhone,
                FirstName = "First " + RandomValues.CreateBase64String(4),
                LastName = "Last " + RandomValues.CreateBase64String(4),
                LockoutEnabled = false,
                HumanBeing = true,
                Immutable = false,
            }, Strings.ApiUnitTestUserPassCurrent);

            await _uow.UserRepo.SetConfirmedEmailAsync(user1.Id, true);
            await _uow.UserRepo.SetConfirmedPasswordAsync(user1.Id, true);
            await _uow.UserRepo.SetConfirmedPhoneNumberAsync(user1.Id, true);
            await _uow.CommitAsync();

            //create test user 2
            user2 = await _uow.UserRepo.CreateAsync(new UserCreate()
            {
                Email = Strings.ApiUnitTestUser2,
                PhoneNumber = Strings.ApiDefaultPhone,
                FirstName = "First " + RandomValues.CreateBase64String(4),
                LastName = "Last " + RandomValues.CreateBase64String(4),
                LockoutEnabled = false,
                Immutable = false,
            }, Strings.ApiUnitTestUserPassCurrent);

            await _uow.UserRepo.SetConfirmedEmailAsync(user2.Id, true);
            await _uow.UserRepo.SetConfirmedPasswordAsync(user2.Id, true);
            await _uow.UserRepo.SetConfirmedPhoneNumberAsync(user2.Id, true);
            await _uow.CommitAsync();

            //assign roles, claims & logins to user 1
            if (!await _uow.UserRepo.IsInRoleAsync(user1.Id, role1.Id))
                await _uow.UserRepo.AddToRoleAsync(user1, role1);

            if (!await _uow.UserRepo.IsInLoginAsync(user1.Id, login1.Id))
                await _uow.UserRepo.AddToLoginAsync(user1, login1);

            if (!await _uow.UserRepo.IsInClaimAsync(user1.Id, claim1.Id))
                await _uow.UserRepo.AddToClaimAsync(user1, claim1);

            await _uow.CommitAsync();

            //assign roles, claims & logins to user 2
            if (!await _uow.UserRepo.IsInRoleAsync(user2.Id, role2.Id))
                await _uow.UserRepo.AddToRoleAsync(user2, role2);

            if (!await _uow.UserRepo.IsInLoginAsync(user2.Id, login2.Id))
                await _uow.UserRepo.AddToLoginAsync(user2, login2);

            if (!await _uow.UserRepo.IsInClaimAsync(user2.Id, claim2.Id))
                await _uow.UserRepo.AddToClaimAsync(user2, claim2);

            await _uow.CommitAsync();
        }

        public async Task CreateRandomAsync(uint sets)
        {
            if (_uow.Situation != ExecutionType.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                AppIssuer issuer;
                AppClient client;
                AppClientUri uri;
                AppRole role;
                AppLogin login;
                AppUser user;
                AppClaim claim;

                //create random issuer
                issuer = await _uow.IssuerRepo.CreateAsync(new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer1 + "-" + RandomValues.CreateBase64String(4),
                    IssuerKey = Strings.ApiUnitTestIssuer1Key,
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random client
                client = await _uow.ClientRepo.CreateAsync(new ClientCreate()
                {
                    IssuerId = issuer.Id,
                    Name = Strings.ApiUnitTestClient1 + "-" + RandomValues.CreateBase64String(4),
                    ClientKey = Strings.ApiUnitTestClient1Key,
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //assign random uri to random client
                uri = await _uow.ClientRepo.CreateUriAsync(new ClientUriCreate()
                {
                    ClientId = client.Id,
                    AbsoluteUri = Strings.ApiUnitTestUri1Link,
                    Enabled = true,
                });

                await _uow.CommitAsync();

                //create random claim
                claim = await _uow.ClaimRepo.CreateAsync(new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Strings.ApiUnitTestClaim1 + "-" + RandomValues.CreateBase64String(4),
                    Value = RandomValues.CreateBase64String(8),
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random login
                login = await _uow.LoginRepo.CreateAsync(new LoginCreate()
                {
                    Name = Strings.ApiUnitTestLogin1 + "-" + RandomValues.CreateBase64String(4),
                    LoginKey = Strings.ApiUnitTestLogin1Key,
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random role
                role = await _uow.RoleRepo.CreateAsync(new RoleCreate()
                {
                    ClientId = client.Id,
                    Name = Strings.ApiUnitTestRole1 + "-" + RandomValues.CreateBase64String(4),
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random user
                user = await _uow.UserRepo.CreateAsync(new UserCreate()
                {
                    Email = RandomValues.CreateAlphaNumericString(4) + "-" + Strings.ApiUnitTestUser1,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }, Strings.ApiUnitTestUserPassCurrent);

                //assign roles, claims & logins to random user
                await _uow.UserRepo.SetConfirmedEmailAsync(user.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(user.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(user.Id, true);
                await _uow.CommitAsync();

                if (!await _uow.UserRepo.IsInRoleAsync(user.Id, role.Id))
                    await _uow.UserRepo.AddToRoleAsync(user, role);

                if (!await _uow.UserRepo.IsInLoginAsync(user.Id, login.Id))
                    await _uow.UserRepo.AddToLoginAsync(user, login);

                if (!await _uow.UserRepo.IsInClaimAsync(user.Id, claim.Id))
                    await _uow.UserRepo.AddToClaimAsync(user, claim);

                await _uow.CommitAsync();
            }
        }

        public async Task DestroyAsync()
        {
            if (_uow.Situation != ExecutionType.UnitTest)
                throw new InvalidOperationException();

            //delete test users
            var users = await _uow.UserRepo.GetAsync(x => x.Email.Contains(Strings.ApiUnitTestUser1)
                || x.Email.Contains(Strings.ApiUnitTestUser2));

            foreach (var user in users)
                await _uow.UserRepo.DeleteAsync(user.Id);

            await _uow.CommitAsync();

            //delete test claims
            var claims = await _uow.ClaimRepo.GetAsync(x => x.Type.Contains(Strings.ApiUnitTestClaim1)
                || x.Type.Contains(Strings.ApiUnitTestClaim2));

            foreach (var claim in claims)
                await _uow.ClaimRepo.DeleteAsync(claim.Id);

            await _uow.CommitAsync();

            //delete test roles
            var roles = await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestRole1)
                || x.Name.Contains(Strings.ApiUnitTestRole2));

            foreach (var role in roles)
                await _uow.RoleRepo.DeleteAsync(role.Id);

            await _uow.CommitAsync();

            //delete test logins
            var logins = await _uow.LoginRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestLogin1)
                || x.Name.Contains(Strings.ApiUnitTestLogin2));

            foreach (var login in logins)
                await _uow.LoginRepo.DeleteAsync(login.Id);

            await _uow.CommitAsync();

            //delete test clients
            var clients = await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestClient1)
                || x.Name.Contains(Strings.ApiUnitTestClient2));

            foreach (var client in clients)
            {
                await _uow.ClientRepo.RemoveRefreshTokensAsync(client);
                await _uow.ClientRepo.DeleteAsync(client.Id);
            }

            await _uow.CommitAsync();

            //delete test issuers
            var issuers = await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestIssuer1)
                || x.Name.Contains(Strings.ApiUnitTestIssuer2));

            foreach (var issuer in issuers)
                await _uow.IssuerRepo.DeleteAsync(issuer.Id);

            await _uow.CommitAsync();
        }
    }
}
