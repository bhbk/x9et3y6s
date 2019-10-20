using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.Lib.Identity.Domain.Tests.Helpers
{
    public class TestData
    {
        private readonly IUoWService _uow;
        private readonly IMapper _mapper;

        public TestData(IUoWService uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public async ValueTask CreateAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * create test settings
             */

            var foundGlobalLegacyClaims = (await _uow.Settings.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyClaims)).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = await _uow.Settings.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalLegacyIssuer = (await _uow.Settings.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyIssuer)).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = await _uow.Settings.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalTotpExpire = (await _uow.Settings.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = await _uow.Settings.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        Immutable = true,
                    }));
            }

            /*
             * create test issuers
             */

            var foundIssuer = (await _uow.Issuers.GetAsync(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name == FakeConstants.ApiTestIssuer)
                .ToLambda())).SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = await _uow.Issuers.CreateAsync(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = FakeConstants.ApiTestIssuer,
                        IssuerKey = FakeConstants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test clients
             */

            var foundClient = (await _uow.Clients.GetAsync(new QueryExpression<tbl_Clients>()
                .Where(x => x.Name == FakeConstants.ApiTestClient)
                .ToLambda())).SingleOrDefault();

            if (foundClient == null)
            {
                foundClient = await _uow.Clients.CreateAsync(
                    _mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = FakeConstants.ApiTestClient,
                        ClientKey = FakeConstants.ApiTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.Activities.CreateAsync(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        ClientId = foundClient.Id,
                        ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                await _uow.Refreshes.CreateAsync(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        ClientId = foundClient.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test client urls
             */

            var url = new Uri(FakeConstants.ApiTestUriLink);

            var foundClientUrl = (await _uow.Urls.GetAsync(new QueryExpression<tbl_Urls>()
                .Where(x => x.ClientId == foundClient.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath)
                .ToLambda())).SingleOrDefault();

            if (foundClientUrl == null)
            {
                foundClientUrl = await _uow.Urls.CreateAsync(
                    _mapper.Map<tbl_Urls>(new UrlCreate()
                    {
                        ClientId = foundClient.Id,
                        UrlHost = url.Scheme + "://" + url.Host,
                        UrlPath = url.AbsolutePath,
                        Enabled = true,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test claims
             */

            var foundClaim = (await _uow.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type == FakeConstants.ApiTestClaim)
                .ToLambda())).SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = await _uow.Claims.CreateAsync(
                    _mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Type = FakeConstants.ApiTestClaim,
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test logins
             */

            var foundLogin = (await _uow.Logins.GetAsync(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == FakeConstants.ApiTestLogin)
                .ToLambda())).SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = await _uow.Logins.CreateAsync(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = FakeConstants.ApiTestLogin,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test roles
             */

            var foundRole = (await _uow.Roles.GetAsync(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == FakeConstants.ApiTestRole)
                .ToLambda())).SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = await _uow.Roles.CreateAsync(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = foundClient.Id,
                        Name = FakeConstants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test users
             */

            var foundUser = (await _uow.Users.GetAsync(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == FakeConstants.ApiTestUser)
                .ToLambda())).SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = await _uow.Users.CreateAsync(
                _mapper.Map<tbl_Users>(new UserCreate()
                {
                    Email = FakeConstants.ApiTestUser,
                    PhoneNumber = FakeConstants.ApiTestUserPhone,
                    FirstName = "First-" + Base64.CreateString(4),
                    LastName = "Last-" + Base64.CreateString(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }), FakeConstants.ApiTestUserPassCurrent);

                await _uow.Activities.CreateAsync(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        ClientId = foundClient.Id,
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                await _uow.Refreshes.CreateAsync(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        ClientId = foundClient.Id,
                        UserId = foundUser.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                await _uow.States.CreateAsync(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        ClientId = foundClient.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                await _uow.States.CreateAsync(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        ClientId = foundClient.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.User.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                await _uow.Users.SetConfirmedEmailAsync(foundUser, true);
                await _uow.Users.SetConfirmedPasswordAsync(foundUser, true);
                await _uow.Users.SetConfirmedPhoneNumberAsync(foundUser, true);

                await _uow.CommitAsync();
            }

            /*
             * assign roles, claims & logins to users
             */

            if (!await _uow.Users.IsInRoleAsync(foundUser.Id, foundRole.Id))
                await _uow.Users.AddToRoleAsync(foundUser, foundRole);

            if (!await _uow.Users.IsInLoginAsync(foundUser.Id, foundLogin.Id))
                await _uow.Users.AddToLoginAsync(foundUser, foundLogin);

            if (!await _uow.Users.IsInClaimAsync(foundUser.Id, foundClaim.Id))
                await _uow.Users.AddToClaimAsync(foundUser, foundClaim);

            await _uow.CommitAsync();
        }

        public async ValueTask CreateAsync(uint sets)
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                tbl_Issuers issuer;
                tbl_Clients client;
                tbl_Urls url;
                tbl_Roles role;
                tbl_Logins login;
                tbl_Users user;
                tbl_Claims claim;

                /*
                 * create random issuers
                 */

                issuer = await _uow.Issuers.CreateAsync(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = FakeConstants.ApiTestIssuer + "-" + Base64.CreateString(4),
                        IssuerKey = FakeConstants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random clients
                 */

                client = await _uow.Clients.CreateAsync(
                    _mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = FakeConstants.ApiTestClient + "-" + Base64.CreateString(4),
                        ClientKey = FakeConstants.ApiTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.Activities.CreateAsync(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        ClientId = client.Id,
                        ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                await _uow.Refreshes.CreateAsync(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                await _uow.CommitAsync();

                /*
                 * create random client urls
                 */

                var clientUrl = new Uri(FakeConstants.ApiTestUriLink);

                url = await _uow.Urls.CreateAsync(
                    _mapper.Map<tbl_Urls>(new UrlCreate()
                    {
                        ClientId = client.Id,
                        UrlHost = clientUrl.Scheme + "://" + clientUrl.Host,
                        UrlPath = clientUrl.AbsolutePath,
                        Enabled = true,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random claims
                 */

                claim = await _uow.Claims.CreateAsync(
                    _mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = FakeConstants.ApiTestClaim + "-" + Base64.CreateString(4),
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random logins
                 */

                login = await _uow.Logins.CreateAsync(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = FakeConstants.ApiTestLogin + "-" + Base64.CreateString(4),
                        LoginKey = FakeConstants.ApiTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random roles
                 */

                role = await _uow.Roles.CreateAsync(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = FakeConstants.ApiTestRole + "-" + Base64.CreateString(4),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random users
                 */

                user = await _uow.Users.CreateAsync(
                    _mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = AlphaNumeric.CreateString(4) + "-" + FakeConstants.ApiTestUser,
                        PhoneNumber = FakeConstants.ApiTestUserPhone + NumberAs.CreateString(1),
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }), FakeConstants.ApiTestUserPassCurrent);

                await _uow.Activities.CreateAsync(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        ClientId = client.Id,
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                await _uow.States.CreateAsync(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                await _uow.Refreshes.CreateAsync(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                /*
                 * assign roles, claims & logins to random users
                 */

                await _uow.Users.SetConfirmedEmailAsync(user, true);
                await _uow.Users.SetConfirmedPasswordAsync(user, true);
                await _uow.Users.SetConfirmedPhoneNumberAsync(user, true);
                await _uow.CommitAsync();

                if (!await _uow.Users.IsInRoleAsync(user.Id, role.Id))
                    await _uow.Users.AddToRoleAsync(user, role);

                if (!await _uow.Users.IsInLoginAsync(user.Id, login.Id))
                    await _uow.Users.AddToLoginAsync(user, login);

                if (!await _uow.Users.IsInClaimAsync(user.Id, claim.Id))
                    await _uow.Users.AddToClaimAsync(user, claim);

                await _uow.CommitAsync();
            }
        }

        public async ValueTask CreateMOTDAsync(uint sets)
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                await _uow.MOTDs.CreateAsync(
                    new tbl_MotDType1()
                    {
                        Id = AlphaNumeric.CreateString(8),
                        Date = DateTime.Now,
                        Author = "Test Author",
                        Quote = "Test Quote",
                        Length = 666,
                        Category = "Test Category",
                        Title = "Test Title",
                        Background = "Test Background",
                        Tags = "tag1,tag2,tag3",
                    });

                await _uow.CommitAsync();
            }
        }

        public async ValueTask DestroyAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * delete test users
             */

            await _uow.Users.DeleteAsync(new QueryExpression<tbl_Users>()
                .Where(x => x.Email.Contains(FakeConstants.ApiTestUser)).ToLambda());
            await _uow.CommitAsync();

            /*
             * delete test roles
             */

            await _uow.Roles.DeleteAsync(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestRole)).ToLambda());
            await _uow.CommitAsync();

            /*
             * delete test logins
             */

            await _uow.Logins.DeleteAsync(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestLogin)).ToLambda());
            await _uow.CommitAsync();

            /*
             * delete test claims
             */

            await _uow.Claims.DeleteAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type.Contains(FakeConstants.ApiTestClaim)).ToLambda());
            await _uow.CommitAsync();

            /*
             * delete test clients
             */

            await _uow.Clients.DeleteAsync(new QueryExpression<tbl_Clients>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestClient)).ToLambda());
            await _uow.CommitAsync();

            /*
             * delete test issuers
             */

            await _uow.Issuers.DeleteAsync(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestIssuer)).ToLambda());
            await _uow.CommitAsync();

            /*
             * delete test msg of the day
             */

            await _uow.MOTDs.DeleteAsync(new QueryExpression<tbl_MotDType1>().ToLambda());
            await _uow.CommitAsync();
        }
    }
}
