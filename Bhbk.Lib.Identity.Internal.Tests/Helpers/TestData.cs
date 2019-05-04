using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Tests.Helpers
{
    public class TestData
    {
        private readonly IUnitOfWork _uow;

        public TestData(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();
        }

        public async Task CreateAsync()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * create test issuer
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = await _uow.IssuerRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiUnitTestIssuer,
                        IssuerKey = Constants.ApiUnitTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test client
             */

            var foundClient = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).SingleOrDefault();

            if (foundClient == null)
            {
                foundClient = await _uow.ClientRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiUnitTestClient,
                        ClientKey = Constants.ApiUnitTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                for (int i = 0; i < 3; i++)
                    await _uow.ActivityRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = foundClient.Id,
                            ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int i = 0; i < 3; i++)
                    await _uow.RefreshRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                        {
                            IssuerId = foundIssuer.Id,
                            ClientId = foundClient.Id,
                            RefreshType = RefreshType.Client.ToString(),
                            RefreshValue = RandomValues.CreateBase64String(8),
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddMinutes(_uow.ConfigRepo.ResourceOwnerRefreshExpire),
                        }));

                await _uow.CommitAsync();
            }

            /*
             * create test client url
             */

            var url = new Uri(Constants.ApiUnitTestUriLink);

            var foundClientUrl = (await _uow.ClientRepo.GetUrlsAsync(x => x.ClientId == foundClient.Id
                && x.UrlHost == (url.Scheme + "://" + url.Host)
                && x.UrlPath == url.AbsolutePath)).SingleOrDefault();

            if (foundClientUrl == null)
            {
                foundClientUrl = await _uow.ClientRepo.CreateUrlAsync(
                    _uow.Mapper.Map<tbl_Urls>(new UrlCreate()
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

            var foundClaim = (await _uow.ClaimRepo.GetAsync(x => x.Type == Constants.ApiUnitTestClaim)).SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = await _uow.ClaimRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Type = Constants.ApiUnitTestClaim,
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test login
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == Constants.ApiUnitTestLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = await _uow.LoginRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = Constants.ApiUnitTestLogin,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test role
             */

            var foundRole = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiUnitTestRole)).SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = await _uow.RoleRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = foundClient.Id,
                        Name = Constants.ApiUnitTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create test user
             */

            var foundUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = await _uow.UserRepo.CreateAsync(
                _uow.Mapper.Map<tbl_Users>(new UserCreate()
                {
                    Email = Constants.ApiUnitTestUser,
                    PhoneNumber = Constants.ApiUnitTestUserPhone,
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }), Constants.ApiUnitTestUserPassCurrent);

                for (int i = 0; i < 3; i++)
                    await _uow.ActivityRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = foundClient.Id,
                            UserId = foundUser.Id,
                            ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int i = 0; i < 3; i++)
                    await _uow.StateRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = foundIssuer.Id,
                            ClientId = foundClient.Id,
                            UserId = foundUser.Id,
                            StateValue = RandomValues.CreateBase64String(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(_uow.ConfigRepo.DeviceCodeTokenExpire),
                        }));

                for (int i = 0; i < 3; i++)
                    await _uow.RefreshRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                        {
                            IssuerId = foundIssuer.Id,
                            ClientId = foundClient.Id,
                            UserId = foundUser.Id,
                            RefreshType = RefreshType.User.ToString(),
                            RefreshValue = RandomValues.CreateBase64String(8),
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddMinutes(_uow.ConfigRepo.ResourceOwnerRefreshExpire),
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
                await _uow.UserRepo.AddRoleAsync(foundUser, foundRole);

            if (!await _uow.UserRepo.IsInLoginAsync(foundUser.Id, foundLogin.Id))
                await _uow.UserRepo.AddLoginAsync(foundUser, foundLogin);

            if (!await _uow.UserRepo.IsInClaimAsync(foundUser.Id, foundClaim.Id))
                await _uow.UserRepo.AddClaimAsync(foundUser, foundClaim);

            await _uow.CommitAsync();

            /*
             * create test msg of the day
             */

            await _uow.UserRepo.CreateMOTDAsync(
                new tbl_MotD_Type1()
                {
                    Id = RandomValues.CreateAlphaNumericString(8),
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
                    _uow.Mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiUnitTestIssuer + "-" + RandomValues.CreateBase64String(4),
                        IssuerKey = Constants.ApiUnitTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random client
                 */

                client = await _uow.ClientRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = Constants.ApiUnitTestClient + "-" + RandomValues.CreateBase64String(4),
                        ClientKey = Constants.ApiUnitTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                for (int j = 0; j < 3; j++)
                    await _uow.ActivityRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = client.Id,
                            ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int j = 0; j < 3; j++)
                    await _uow.RefreshRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                        {
                            IssuerId = issuer.Id,
                            ClientId = client.Id,
                            RefreshType = RefreshType.Client.ToString(),
                            RefreshValue = RandomValues.CreateBase64String(8),
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddMinutes(_uow.ConfigRepo.ResourceOwnerRefreshExpire),
                        }));

                await _uow.CommitAsync();

                /*
                 * create random client url
                 */

                var clientUrl = new Uri(Constants.ApiUnitTestUriLink);

                url = await _uow.ClientRepo.CreateUrlAsync(
                    _uow.Mapper.Map<tbl_Urls>(new UrlCreate()
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
                    _uow.Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = Constants.ApiUnitTestClaim + "-" + RandomValues.CreateBase64String(4),
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random login
                 */

                login = await _uow.LoginRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = Constants.ApiUnitTestLogin + "-" + RandomValues.CreateBase64String(4),
                        LoginKey = Constants.ApiUnitTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random role
                 */

                role = await _uow.RoleRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = Constants.ApiUnitTestRole + "-" + RandomValues.CreateBase64String(4),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                /*
                 * create random user
                 */

                user = await _uow.UserRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = RandomValues.CreateAlphaNumericString(4) + "-" + Constants.ApiUnitTestUser,
                        PhoneNumber = Constants.ApiUnitTestUserPhone + RandomValues.CreateNumberAsString(1),
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }), Constants.ApiUnitTestUserPassCurrent);

                for (int j = 0; j < 3; j++)
                    await _uow.ActivityRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Activities>(new ActivityCreate()
                        {
                            ClientId = client.Id,
                            UserId = user.Id,
                            ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                            Immutable = false,
                        }));

                for (int j = 0; j < 3; j++)
                    await _uow.StateRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = issuer.Id,
                            ClientId = client.Id,
                            UserId = user.Id,
                            StateValue = RandomValues.CreateBase64String(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(_uow.ConfigRepo.DeviceCodeTokenExpire),
                        }));

                for (int j = 0; j < 3; j++)
                    await _uow.RefreshRepo.CreateAsync(
                        _uow.Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                        {
                            IssuerId = issuer.Id,
                            ClientId = client.Id,
                            UserId = user.Id,
                            RefreshType = RefreshType.User.ToString(),
                            RefreshValue = RandomValues.CreateBase64String(8),
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddMinutes(_uow.ConfigRepo.ResourceOwnerRefreshExpire),
                        }));

                /*
                 * assign roles, claims & logins to random user
                 */

                await _uow.UserRepo.SetConfirmedEmailAsync(user.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(user.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(user.Id, true);
                await _uow.CommitAsync();

                if (!await _uow.UserRepo.IsInRoleAsync(user.Id, role.Id))
                    await _uow.UserRepo.AddRoleAsync(user, role);

                if (!await _uow.UserRepo.IsInLoginAsync(user.Id, login.Id))
                    await _uow.UserRepo.AddLoginAsync(user, login);

                if (!await _uow.UserRepo.IsInClaimAsync(user.Id, claim.Id))
                    await _uow.UserRepo.AddClaimAsync(user, claim);

                await _uow.CommitAsync();

                /*
                 * create random msg of the day
                 */

                await _uow.UserRepo.CreateMOTDAsync(
                    new tbl_MotD_Type1()
                    {
                        Id = RandomValues.CreateAlphaNumericString(8),
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

            var users = await _uow.UserRepo.GetAsync(x => x.Email.Contains(Constants.ApiUnitTestUser));

            foreach (var user in users)
                await _uow.UserRepo.DeleteAsync(user.Id);

            await _uow.CommitAsync();

            /*
             * delete test roles
             */

            var roles = await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestRole));

            foreach (var role in roles)
                await _uow.RoleRepo.DeleteAsync(role.Id);

            await _uow.CommitAsync();

            /*
             * delete test logins
             */

            var logins = await _uow.LoginRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestLogin));

            foreach (var login in logins)
                await _uow.LoginRepo.DeleteAsync(login.Id);

            await _uow.CommitAsync();

            /*
             * delete test claims
             */

            var claims = await _uow.ClaimRepo.GetAsync(x => x.Type.Contains(Constants.ApiUnitTestClaim));

            foreach (var claim in claims)
                await _uow.ClaimRepo.DeleteAsync(claim.Id);

            await _uow.CommitAsync();

            /*
             * delete test clients
             */

            var clients = await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestClient));

            foreach (var client in clients)
                await _uow.ClientRepo.DeleteAsync(client.Id);

            await _uow.CommitAsync();

            /*
             * delete test issuers
             */

            var issuers = await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Constants.ApiUnitTestIssuer));

            foreach (var issuer in issuers)
                await _uow.IssuerRepo.DeleteAsync(issuer.Id);

            await _uow.CommitAsync();
        }
    }
}
