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

        public void Create()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * create test settings
             */

            var foundGlobalLegacyClaims = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyIssuer).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
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

            var foundIssuer = _uow.Issuers.Get(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name == FakeConstants.ApiTestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = FakeConstants.ApiTestIssuer,
                        IssuerKey = FakeConstants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test audiences
             */

            var foundClient = _uow.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == FakeConstants.ApiTestAudience).ToLambda())
                .SingleOrDefault();

            if (foundClient == null)
            {
                foundClient = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = FakeConstants.ApiTestAudience,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Activities_Deprecate.Create(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        AudienceId = foundClient.Id,
                        ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundClient.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();
            }

            /*
             * create test client urls
             */

            var url = new Uri(FakeConstants.ApiTestUriLink);

            var foundClientUrl = _uow.Urls.Get(new QueryExpression<tbl_Urls>()
                .Where(x => x.AudienceId == foundClient.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath).ToLambda())
                .SingleOrDefault();

            if (foundClientUrl == null)
            {
                foundClientUrl = _uow.Urls.Create(
                    _mapper.Map<tbl_Urls>(new UrlCreate()
                    {
                        AudienceId = foundClient.Id,
                        UrlHost = url.Scheme + "://" + url.Host,
                        UrlPath = url.AbsolutePath,
                        Enabled = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test claims
             */

            var foundClaim = _uow.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type == FakeConstants.ApiTestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Type = FakeConstants.ApiTestClaim,
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test logins
             */

            var foundLogin = _uow.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == FakeConstants.ApiTestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = FakeConstants.ApiTestLogin,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test roles
             */

            var foundRole = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == FakeConstants.ApiTestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = foundClient.Id,
                        Name = FakeConstants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test users
             */

            var foundUser = _uow.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == FakeConstants.ApiTestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
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

                _uow.Activities_Deprecate.Create(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        AudienceId = foundClient.Id,
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundClient.Id,
                        UserId = foundUser.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundClient.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundClient.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.User.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Users.SetConfirmedEmail(foundUser, true);
                _uow.Users.SetConfirmedPassword(foundUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundUser, true);

                _uow.Commit();
            }

            /*
             * set password to random audiences
             */

            _uow.Audiences.SetPassword(foundClient, FakeConstants.ApiTestAudiencePassCurrent);

            _uow.Commit();

            /*
             * assign roles, claims & logins to users
             */

            if (!_uow.Users.IsInRole(foundUser.Id, foundRole.Id))
                _uow.Users.AddToRole(foundUser, foundRole);

            if (!_uow.Users.IsInLogin(foundUser.Id, foundLogin.Id))
                _uow.Users.AddToLogin(foundUser, foundLogin);

            if (!_uow.Users.IsInClaim(foundUser.Id, foundClaim.Id))
                _uow.Users.AddToClaim(foundUser, foundClaim);

            _uow.Commit();
        }

        public void Create(uint sets)
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                tbl_Issuers issuer;
                tbl_Audiences audience;
                tbl_Urls url;
                tbl_Roles role;
                tbl_Logins login;
                tbl_Users user;
                tbl_Claims claim;

                /*
                 * create random issuers
                 */

                issuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = FakeConstants.ApiTestIssuer + "-" + Base64.CreateString(4),
                        IssuerKey = FakeConstants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random audiences
                 */

                audience = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = FakeConstants.ApiTestAudience + "-" + Base64.CreateString(4),
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Activities_Deprecate.Create(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        AudienceId = audience.Id,
                        ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();

                /*
                 * create random audience urls
                 */

                var audienceUrl = new Uri(FakeConstants.ApiTestUriLink);

                url = _uow.Urls.Create(
                    _mapper.Map<tbl_Urls>(new UrlCreate()
                    {
                        AudienceId = audience.Id,
                        UrlHost = audienceUrl.Scheme + "://" + audienceUrl.Host,
                        UrlPath = audienceUrl.AbsolutePath,
                        Enabled = true,
                    }));

                _uow.Commit();

                /*
                 * create random claims
                 */

                claim = _uow.Claims.Create(
                    _mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = FakeConstants.ApiTestClaim + "-" + Base64.CreateString(4),
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random logins
                 */

                login = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = FakeConstants.ApiTestLogin + "-" + Base64.CreateString(4),
                        LoginKey = FakeConstants.ApiTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random roles
                 */

                role = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = audience.Id,
                        Name = FakeConstants.ApiTestRole + "-" + Base64.CreateString(4),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random users
                 */

                user = _uow.Users.Create(
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

                _uow.Activities_Deprecate.Create(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                /*
                 * set password for random audiences
                 */

                _uow.Audiences.SetPassword(audience, FakeConstants.ApiTestAudiencePassCurrent);

                _uow.Commit();

                /*
                 * assign roles, claims & logins to random users
                 */

                _uow.Users.SetConfirmedEmail(user, true);
                _uow.Users.SetConfirmedPassword(user, true);
                _uow.Users.SetConfirmedPhoneNumber(user, true);
                _uow.Commit();

                if (!_uow.Users.IsInRole(user.Id, role.Id))
                    _uow.Users.AddToRole(user, role);

                if (!_uow.Users.IsInLogin(user.Id, login.Id))
                    _uow.Users.AddToLogin(user, login);

                if (!_uow.Users.IsInClaim(user.Id, claim.Id))
                    _uow.Users.AddToClaim(user, claim);

                _uow.Commit();
            }
        }

        public void CreateMOTD(uint sets)
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                _uow.MOTDs.Create(
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

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * delete test users
             */

            _uow.Users.Delete(new QueryExpression<tbl_Users>()
                .Where(x => x.Email.Contains(FakeConstants.ApiTestUser)).ToLambda());
            _uow.Commit();

            /*
             * delete test roles
             */

            _uow.Roles.Delete(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestRole)).ToLambda());
            _uow.Commit();

            /*
             * delete test logins
             */

            _uow.Logins.Delete(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestLogin)).ToLambda());
            _uow.Commit();

            /*
             * delete test claims
             */

            _uow.Claims.Delete(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type.Contains(FakeConstants.ApiTestClaim)).ToLambda());
            _uow.Commit();

            /*
             * delete test audiences
             */

            _uow.Audiences.Delete(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestAudience)).ToLambda());
            _uow.Commit();

            /*
             * delete test issuers
             */

            _uow.Issuers.Delete(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name.Contains(FakeConstants.ApiTestIssuer)).ToLambda());
            _uow.Commit();

            /*
             * delete test msg of the day
             */

            _uow.MOTDs.Delete(new QueryExpression<tbl_MotDType1>().ToLambda());
            _uow.Commit();
        }
    }
}
