using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
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

        public async Task CreateAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * create settings
             */

            var foundGlobalLegacyClaims = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyClaims)).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = await _uow.SettingRepo.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalLegacyIssuer = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyIssuer)).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = await _uow.SettingRepo.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalTotpExpire = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = await _uow.SettingRepo.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        Immutable = true,
                    }));
            }

            /*
             * create test issuer
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = await _uow.IssuerRepo.CreateAsync(
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
             * create test client
             */

            var foundClient = (await _uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).SingleOrDefault();

            if (foundClient == null)
            {
                foundClient = await _uow.ClientRepo.CreateAsync(
                    _mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = FakeConstants.ApiTestClient,
                        ClientKey = FakeConstants.ApiTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                for (int i = 0; i < 3; i++)
                    await _uow.ActivityRepo.CreateAsync(
                        _mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = foundClient.Id,
                            ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int i = 0; i < 3; i++)
                    await _uow.RefreshRepo.CreateAsync(
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
             * create test client url
             */

            var url = new Uri(FakeConstants.ApiTestUriLink);

            var foundClientUrl = (await _uow.ClientRepo.GetUrlsAsync(x => x.ClientId == foundClient.Id
                && x.UrlHost == (url.Scheme + "://" + url.Host)
                && x.UrlPath == url.AbsolutePath)).SingleOrDefault();

            if (foundClientUrl == null)
            {
                foundClientUrl = await _uow.ClientRepo.CreateUrlAsync(
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
             * create test claim
             */

            var foundClaim = (await _uow.ClaimRepo.GetAsync(x => x.Type == FakeConstants.ApiTestClaim)).SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = await _uow.ClaimRepo.CreateAsync(
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
             * create test login
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == FakeConstants.ApiTestLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = await _uow.LoginRepo.CreateAsync(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = FakeConstants.ApiTestLogin,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test role
             */

            var foundRole = (await _uow.RoleRepo.GetAsync(x => x.Name == FakeConstants.ApiTestRole)).SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = await _uow.RoleRepo.CreateAsync(
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
             * create test user
             */

            var foundUser = (await _uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = await _uow.UserRepo.CreateAsync(
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

                for (int i = 0; i < 3; i++)
                    await _uow.ActivityRepo.CreateAsync(
                        _mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = foundClient.Id,
                            UserId = foundUser.Id,
                            ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int i = 0; i < 3; i++)
                    await _uow.StateRepo.CreateAsync(
                        _mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = foundIssuer.Id,
                            ClientId = foundClient.Id,
                            UserId = foundUser.Id,
                            StateValue = Base64.CreateString(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        }));

                for (int i = 0; i < 3; i++)
                    await _uow.RefreshRepo.CreateAsync(
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

                await _uow.UserRepo.SetConfirmedEmailAsync(foundUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundUser.Id, true);
                await _uow.CommitAsync();
            }

            /*
             * assign roles, claims & logins to user
             */

            if (!await _uow.UserRepo.IsInRoleAsync(foundUser.Id, foundRole.Id))
                await _uow.UserRepo.AddToRoleAsync(foundUser, foundRole);

            if (!await _uow.UserRepo.IsInLoginAsync(foundUser.Id, foundLogin.Id))
                await _uow.UserRepo.AddToLoginAsync(foundUser, foundLogin);

            if (!await _uow.UserRepo.IsInClaimAsync(foundUser.Id, foundClaim.Id))
                await _uow.UserRepo.AddToClaimAsync(foundUser, foundClaim);

            await _uow.CommitAsync();

            /*
             * create test msg of the day
             */

            await _uow.UserRepo.CreateMOTDAsync(
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

        public async Task CreateRandomAsync(uint sets)
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
                 * create random issuer
                 */

                issuer = await _uow.IssuerRepo.CreateAsync(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = FakeConstants.ApiTestIssuer + "-" + Base64.CreateString(4),
                        IssuerKey = FakeConstants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random client
                 */

                client = await _uow.ClientRepo.CreateAsync(
                    _mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = FakeConstants.ApiTestClient + "-" + Base64.CreateString(4),
                        ClientKey = FakeConstants.ApiTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                for (int j = 0; j < 3; j++)
                    await _uow.ActivityRepo.CreateAsync(
                        _mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = client.Id,
                            ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int j = 0; j < 3; j++)
                    await _uow.RefreshRepo.CreateAsync(
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
                 * create random client url
                 */

                var clientUrl = new Uri(FakeConstants.ApiTestUriLink);

                url = await _uow.ClientRepo.CreateUrlAsync(
                    _mapper.Map<tbl_Urls>(new UrlCreate()
                    {
                        ClientId = client.Id,
                        UrlHost = clientUrl.Scheme + "://" + clientUrl.Host,
                        UrlPath = clientUrl.AbsolutePath,
                        Enabled = true,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random claim
                 */

                claim = await _uow.ClaimRepo.CreateAsync(
                    _mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = FakeConstants.ApiTestClaim + "-" + Base64.CreateString(4),
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random login
                 */

                login = await _uow.LoginRepo.CreateAsync(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = FakeConstants.ApiTestLogin + "-" + Base64.CreateString(4),
                        LoginKey = FakeConstants.ApiTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random role
                 */

                role = await _uow.RoleRepo.CreateAsync(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = FakeConstants.ApiTestRole + "-" + Base64.CreateString(4),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random user
                 */

                user = await _uow.UserRepo.CreateAsync(
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

                for (int j = 0; j < 3; j++)
                    await _uow.ActivityRepo.CreateAsync(
                        _mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = client.Id,
                            UserId = user.Id,
                            ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int j = 0; j < 3; j++)
                    await _uow.StateRepo.CreateAsync(
                        _mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = issuer.Id,
                            ClientId = client.Id,
                            UserId = user.Id,
                            StateValue = Base64.CreateString(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        }));

                for (int j = 0; j < 3; j++)
                    await _uow.RefreshRepo.CreateAsync(
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
                 * assign roles, claims & logins to random user
                 */

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

                /*
                 * create random msg of the day
                 */

                await _uow.UserRepo.CreateMOTDAsync(
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

        public async Task DestroyAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * delete test users
             */

            var users = await _uow.UserRepo.GetAsync(x => x.Email.Contains(FakeConstants.ApiTestUser));

            foreach (var user in users)
                await _uow.UserRepo.DeleteAsync(user.Id);

            await _uow.CommitAsync();

            /*
             * delete test roles
             */

            var roles = await _uow.RoleRepo.GetAsync(x => x.Name.Contains(FakeConstants.ApiTestRole));

            foreach (var role in roles)
                await _uow.RoleRepo.DeleteAsync(role.Id);

            await _uow.CommitAsync();

            /*
             * delete test logins
             */

            var logins = await _uow.LoginRepo.GetAsync(x => x.Name.Contains(FakeConstants.ApiTestLogin));

            foreach (var login in logins)
                await _uow.LoginRepo.DeleteAsync(login.Id);

            await _uow.CommitAsync();

            /*
             * delete test claims
             */

            var claims = await _uow.ClaimRepo.GetAsync(x => x.Type.Contains(FakeConstants.ApiTestClaim));

            foreach (var claim in claims)
                await _uow.ClaimRepo.DeleteAsync(claim.Id);

            await _uow.CommitAsync();

            /*
             * delete test clients
             */

            var clients = await _uow.ClientRepo.GetAsync(x => x.Name.Contains(FakeConstants.ApiTestClient));

            foreach (var client in clients)
                await _uow.ClientRepo.DeleteAsync(client.Id);

            await _uow.CommitAsync();

            /*
             * delete test issuers
             */

            var issuers = await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(FakeConstants.ApiTestIssuer));

            foreach (var issuer in issuers)
                await _uow.IssuerRepo.DeleteAsync(issuer.Id);

            await _uow.CommitAsync();
        }
    }
}
