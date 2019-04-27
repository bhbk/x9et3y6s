using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Tests.Helpers
{
    public class TestData
    {
        private readonly IIdentityUnitOfWork _uow;

        public TestData(IIdentityUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();
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
                Name = Constants.ApiUnitTestIssuer,
                IssuerKey = Constants.ApiUnitTestIssuerKey,
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test clients
            client = await _uow.ClientRepo.CreateAsync(new ClientCreate()
            {
                IssuerId = issuer.Id,
                Name = Constants.ApiUnitTestClient,
                ClientKey = Constants.ApiUnitTestClientKey,
                ClientType = ClientType.user_agent.ToString(),
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //assign test client uris
            var uri1 = new Uri(Constants.ApiUnitTestUriLink);

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
                Type = Constants.ApiUnitTestClaim,
                Value = RandomValues.CreateBase64String(8),
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test logins
            login = await _uow.LoginRepo.CreateAsync(new LoginCreate()
            {
                Name = Constants.ApiUnitTestLogin,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test roles
            role = await _uow.RoleRepo.CreateAsync(new RoleCreate()
            {
                ClientId = client.Id,
                Name = Constants.ApiUnitTestRole,
                Enabled = true,
                Immutable = false,
            });

            await _uow.CommitAsync();

            //create test user 1
            user = await _uow.UserRepo.CreateAsync(new UserCreate()
            {
                Email = Constants.ApiUnitTestUser,
                PhoneNumber = Constants.ApiUnitTestUserPhone,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                LockoutEnabled = false,
                HumanBeing = true,
                Immutable = false,
            }, Constants.ApiUnitTestUserPassCurrent);

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

            //create activity
            await _uow.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    ClientId = client.Id,
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false,
                });

            //create refreshes
            await _uow.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = RandomValues.CreateBase64String(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddMinutes(_uow.ConfigRepo.ResourceOwnerRefreshExpire),
                });

            //create states
            await _uow.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_uow.ConfigRepo.DeviceCodeTokenExpire),
                });

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
                    Name = Constants.ApiUnitTestIssuer + "-" + RandomValues.CreateBase64String(4),
                    IssuerKey = Constants.ApiUnitTestIssuerKey,
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random client
                client = await _uow.ClientRepo.CreateAsync(new ClientCreate()
                {
                    IssuerId = issuer.Id,
                    Name = Constants.ApiUnitTestClient + "-" + RandomValues.CreateBase64String(4),
                    ClientKey = Constants.ApiUnitTestClientKey,
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //assign random uri to random client
                var clientUri1 = new Uri(Constants.ApiUnitTestUriLink);

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
                    Type = Constants.ApiUnitTestClaim + "-" + RandomValues.CreateBase64String(4),
                    Value = RandomValues.CreateBase64String(8),
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random login
                login = await _uow.LoginRepo.CreateAsync(new LoginCreate()
                {
                    Name = Constants.ApiUnitTestLogin + "-" + RandomValues.CreateBase64String(4),
                    LoginKey = Constants.ApiUnitTestLoginKey,
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random role
                role = await _uow.RoleRepo.CreateAsync(new RoleCreate()
                {
                    ClientId = client.Id,
                    Name = Constants.ApiUnitTestRole + "-" + RandomValues.CreateBase64String(4),
                    Enabled = true,
                    Immutable = false,
                });

                await _uow.CommitAsync();

                //create random user
                user = await _uow.UserRepo.CreateAsync(new UserCreate()
                {
                    Email = RandomValues.CreateAlphaNumericString(4) + "-" + Constants.ApiUnitTestUser,
                    PhoneNumber = Constants.ApiUnitTestUserPhone + RandomValues.CreateNumberAsString(1),
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }, Constants.ApiUnitTestUserPassCurrent);

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

                //create activity
                await _uow.ActivityRepo.CreateAsync(
                    new ActivityCreate()
                    {
                        ClientId = client.Id,
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    });

                //create refreshes
                await _uow.RefreshRepo.CreateAsync(
                    new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = RandomValues.CreateBase64String(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddMinutes(_uow.ConfigRepo.ResourceOwnerRefreshExpire),
                    });

                //create states
                await _uow.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(_uow.ConfigRepo.DeviceCodeTokenExpire),
                    });

                await _uow.CommitAsync();
            }
        }

        public async Task DestroyAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            //delete test activity
            var actvities = await _uow.ActivityRepo.GetAsync();

            foreach (var activity in actvities)
                await _uow.ActivityRepo.DeleteAsync(activity.Id);

            await _uow.CommitAsync();

            //delete test refreshes
            var refreshes = await _uow.RefreshRepo.GetAsync();

            foreach (var refresh in refreshes)
                await _uow.RefreshRepo.DeleteAsync(refresh.Id);

            await _uow.CommitAsync();

            //delete test states
            var states = await _uow.StateRepo.GetAsync();

            foreach (var state in states)
                await _uow.StateRepo.DeleteAsync(state.Id);

            await _uow.CommitAsync();

            //delete test users
            var users = await _uow.UserRepo.GetAsync(x => x.Email.Contains(Constants.ApiUnitTestUser));

            foreach (var user in users)
                await _uow.UserRepo.DeleteAsync(user.Id);

            await _uow.CommitAsync();

            //delete test claims
            var claims = await _uow.ClaimRepo.GetAsync(x => x.Type.Contains(Constants.ApiUnitTestClaim));

            foreach (var claim in claims)
                await _uow.ClaimRepo.DeleteAsync(claim.Id);

            await _uow.CommitAsync();

            //delete test roles
            var roles = await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestRole));

            foreach (var role in roles)
                await _uow.RoleRepo.DeleteAsync(role.Id);

            await _uow.CommitAsync();

            //delete test logins
            var logins = await _uow.LoginRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestLogin));

            foreach (var login in logins)
                await _uow.LoginRepo.DeleteAsync(login.Id);

            await _uow.CommitAsync();

            //delete test clients
            var clients = await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestClient));

            foreach (var client in clients)
                await _uow.ClientRepo.DeleteAsync(client.Id);

            await _uow.CommitAsync();

            //delete test issuers
            var issuers = await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestIssuer));

            foreach (var issuer in issuers)
                await _uow.IssuerRepo.DeleteAsync(issuer.Id);

            await _uow.CommitAsync();
        }
    }
}
