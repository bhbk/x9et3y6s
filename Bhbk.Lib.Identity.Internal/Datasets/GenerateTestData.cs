using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Datasets
{
    public class GenerateTestData
    {
        private readonly IIdentityUnitOfWork<IdentityDbContext> _uow;

        public GenerateTestData(IIdentityUnitOfWork<IdentityDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async Task CreateAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            tbl_Issuers issuer;
            tbl_Clients client;
            tbl_Urls clientUri;
            tbl_Roles role;
            tbl_Logins login;
            tbl_Users user;
            tbl_Claims claim;

            //create test issuers
            issuer = await _uow.IssuerRepo.CreateAsync(new IssuerCreate()
            {
                Name = Strings.ApiUnitTestIssuer,
                IssuerKey = Strings.ApiUnitTestIssuerKey,
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test clients
            client = await _uow.ClientRepo.CreateAsync(new ClientCreate()
            {
                IssuerId = issuer.Id,
                Name = Strings.ApiUnitTestClient,
                ClientKey = Strings.ApiUnitTestClientKey,
                ClientType = ClientType.user_agent.ToString(),
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //assign test client uris
            var uri1 = new Uri(Strings.ApiUnitTestUriLink);

            clientUri = await _uow.ClientRepo.CreateUriAsync(new UrlCreate()
            {
                ClientId = client.Id,
                UrlHost = uri1.Scheme + "://" + uri1.Host,
                UrlPath = uri1.AbsolutePath,
                Enabled = true,
            });

            await _uow.CommitAsync();

            //create test claims
            claim = await _uow.ClaimRepo.CreateAsync(new ClaimCreate()
            {
                IssuerId = issuer.Id,
                Type = Strings.ApiUnitTestClaim,
                Value = RandomValues.CreateBase64String(8),
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test logins
            login = await _uow.LoginRepo.CreateAsync(new LoginCreate()
            {
                Name = Strings.ApiUnitTestLogin,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test roles
            role = await _uow.RoleRepo.CreateAsync(new RoleCreate()
            {
                ClientId = client.Id,
                Name = Strings.ApiUnitTestRole,
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test user 1
            user = await _uow.UserRepo.CreateAsync(new UserCreate()
            {
                Email = Strings.ApiUnitTestUser,
                PhoneNumber = Strings.ApiUnitTestUserPhone,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                LockoutEnabled = false,
                HumanBeing = true,
                Immutable = false,
            }, Strings.ApiUnitTestUserPassCurrent);

            await _uow.UserRepo.SetConfirmedEmailAsync(user.Id, true);
            await _uow.UserRepo.SetConfirmedPasswordAsync(user.Id, true);
            await _uow.UserRepo.SetConfirmedPhoneNumberAsync(user.Id, true);
            await _uow.CommitAsync();

            //assign roles, claims & logins to user 1
            if (!await _uow.UserRepo.IsInRoleAsync(user.Id, role.Id))
                await _uow.UserRepo.AddToRoleAsync(user, role);

            if (!await _uow.UserRepo.IsInLoginAsync(user.Id, login.Id))
                await _uow.UserRepo.AddToLoginAsync(user, login);

            if (!await _uow.UserRepo.IsInClaimAsync(user.Id, claim.Id))
                await _uow.UserRepo.AddToClaimAsync(user, claim);

            await _uow.CommitAsync();
        }

        public async Task CreateRandomAsync(uint sets)
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                tbl_Issuers issuer;
                tbl_Clients client;
                tbl_Urls uri;
                tbl_Roles role;
                tbl_Logins login;
                tbl_Users user;
                tbl_Claims claim;

                //create random issuer
                issuer = await _uow.IssuerRepo.CreateAsync(new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer + "-" + RandomValues.CreateBase64String(4),
                    IssuerKey = Strings.ApiUnitTestIssuerKey,
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random client
                client = await _uow.ClientRepo.CreateAsync(new ClientCreate()
                {
                    IssuerId = issuer.Id,
                    Name = Strings.ApiUnitTestClient + "-" + RandomValues.CreateBase64String(4),
                    ClientKey = Strings.ApiUnitTestClientKey,
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //assign random uri to random client
                var clientUri1 = new Uri(Strings.ApiUnitTestUriLink);

                uri = await _uow.ClientRepo.CreateUriAsync(new UrlCreate()
                {
                    ClientId = client.Id,
                    UrlHost = clientUri1.Scheme + "://" + clientUri1.Host,
                    UrlPath = clientUri1.AbsolutePath,
                    Enabled = true,
                });

                await _uow.CommitAsync();

                //create random claim
                claim = await _uow.ClaimRepo.CreateAsync(new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Strings.ApiUnitTestClaim + "-" + RandomValues.CreateBase64String(4),
                    Value = RandomValues.CreateBase64String(8),
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random login
                login = await _uow.LoginRepo.CreateAsync(new LoginCreate()
                {
                    Name = Strings.ApiUnitTestLogin + "-" + RandomValues.CreateBase64String(4),
                    LoginKey = Strings.ApiUnitTestLoginKey,
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random role
                role = await _uow.RoleRepo.CreateAsync(new RoleCreate()
                {
                    ClientId = client.Id,
                    Name = Strings.ApiUnitTestRole + "-" + RandomValues.CreateBase64String(4),
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random user
                user = await _uow.UserRepo.CreateAsync(new UserCreate()
                {
                    Email = RandomValues.CreateAlphaNumericString(4) + "-" + Strings.ApiUnitTestUser,
                    PhoneNumber = Strings.ApiUnitTestUserPhone + RandomValues.CreateNumberAsString(1),
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
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            //delete test users
            var users = await _uow.UserRepo.GetAsync(x => x.Email.Contains(Strings.ApiUnitTestUser));

            foreach (var user in users)
                await _uow.UserRepo.DeleteAsync(user.Id);

            await _uow.CommitAsync();

            //delete test claims
            var claims = await _uow.ClaimRepo.GetAsync(x => x.Type.Contains(Strings.ApiUnitTestClaim));

            foreach (var claim in claims)
                await _uow.ClaimRepo.DeleteAsync(claim.Id);

            await _uow.CommitAsync();

            //delete test roles
            var roles = await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestRole));

            foreach (var role in roles)
                await _uow.RoleRepo.DeleteAsync(role.Id);

            await _uow.CommitAsync();

            //delete test logins
            var logins = await _uow.LoginRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestLogin));

            foreach (var login in logins)
                await _uow.LoginRepo.DeleteAsync(login.Id);

            await _uow.CommitAsync();

            //delete test clients
            var clients = await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestClient));

            foreach (var client in clients)
                await _uow.ClientRepo.DeleteAsync(client.Id);

            await _uow.CommitAsync();

            //delete test issuers
            var issuers = await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestIssuer));

            foreach (var issuer in issuers)
                await _uow.IssuerRepo.DeleteAsync(issuer.Id);

            await _uow.CommitAsync();
        }
    }
}
