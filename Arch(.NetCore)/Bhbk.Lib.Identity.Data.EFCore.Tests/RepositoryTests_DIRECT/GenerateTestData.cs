using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    public class GenerateTestData
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ValidationHelper _validate;

        public GenerateTestData(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
            _validate = new ValidationHelper();
        }

        public void Create()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test settings
             */
            var foundGlobalLegacyClaims = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<tbl_Settings>(new SettingV1()
                    {
                        ConfigKey = Constants.ApiSettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyIssuer).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _mapper.Map<tbl_Settings>(new SettingV1()
                    {
                        ConfigKey = Constants.ApiSettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalTotpExpire).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
                    _mapper.Map<tbl_Settings>(new SettingV1()
                    {
                        ConfigKey = Constants.ApiSettingGlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        Immutable = true,
                    }));
            }

            /*
             * create test issuers
             */
            var foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuers>(new IssuerV1()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test audiences
             */
            var foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiTestAudience,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activities>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = AlphaNumeric.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();
            }

            /*
             * create test client urls
             */
            var url = new Uri(Constants.ApiTestUriLink);

            var foundAudienceUrl = _uow.Urls.Get(QueryExpressionFactory.GetQueryExpression<tbl_Urls>()
                .Where(x => x.AudienceId == foundAudience.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath).ToLambda())
                .SingleOrDefault();

            if (foundAudienceUrl == null)
            {
                foundAudienceUrl = _uow.Urls.Create(
                    _mapper.Map<tbl_Urls>(new UrlV1()
                    {
                        AudienceId = foundAudience.Id,
                        UrlHost = url.Scheme + "://" + url.Host,
                        UrlPath = url.AbsolutePath,
                        Enabled = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test claims
             */
            var foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _mapper.Map<tbl_Claims>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.ApiTestClaimSubject,
                        Type = Constants.ApiTestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.ApiTestClaimValueType,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test logins
             */
            var foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginV1()
                    {
                        Name = Constants.ApiTestLogin,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test roles
             */
            var foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.ApiTestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserV1()
                    {
                        UserName = Constants.ApiTestUser,
                        Email = Constants.ApiTestUser,
                        PhoneNumber = Constants.ApiTestUserPhone,
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }), Constants.ApiTestUserPassCurrent);

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activities>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = AlphaNumeric.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_States>(new StateV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_States>(new StateV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
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
             * set password to audiences
             */
            _uow.Audiences.SetPasswordHash(foundAudience, Constants.ApiTestAudiencePassCurrent);
            _uow.Commit();

            /*
             * assign roles to audiences
             */
            if (!_uow.Audiences.IsInRole(foundAudience, foundRole))
                _uow.Audiences.AddToRole(foundAudience, foundRole);

            _uow.Commit();

            /*
             * assign roles, claims & logins to users
             */
            if (!_uow.Users.IsInRole(foundUser, foundRole))
                _uow.Users.AddToRole(foundUser, foundRole);

            if (!_uow.Users.IsInLogin(foundUser, foundLogin))
                _uow.Users.AddToLogin(foundUser, foundLogin);

            if (!_uow.Users.IsInClaim(foundUser, foundClaim))
                _uow.Users.AddToClaim(foundUser, foundClaim);

            _uow.Commit();
        }

        public void Create(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
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
                    _mapper.Map<tbl_Issuers>(new IssuerV1()
                    {
                        Name = Constants.ApiTestIssuer + "-" + Base64.CreateString(4),
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random audiences
                 */
                audience = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceV1()
                    {
                        IssuerId = issuer.Id,
                        Name = Constants.ApiTestAudience + "-" + Base64.CreateString(4),
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activities>(new ActivityV1()
                    {
                        AudienceId = audience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = AlphaNumeric.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();

                /*
                 * create random audience urls
                 */
                var audienceUrl = new Uri(Constants.ApiTestUriLink);

                url = _uow.Urls.Create(
                    _mapper.Map<tbl_Urls>(new UrlV1()
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
                    _mapper.Map<tbl_Claims>(new ClaimV1()
                    {
                        IssuerId = issuer.Id,
                        Subject = Constants.ApiTestClaimSubject + "-" + AlphaNumeric.CreateString(4),
                        Type = Constants.ApiTestClaim + "-" + AlphaNumeric.CreateString(4),
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.ApiTestClaimValueType + "-" + AlphaNumeric.CreateString(4),
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random logins
                 */
                login = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginV1()
                    {
                        Name = Constants.ApiTestLogin + "-" + AlphaNumeric.CreateString(4),
                        LoginKey = Constants.ApiTestLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random roles
                 */
                role = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleV1()
                    {
                        AudienceId = audience.Id,
                        Name = Constants.ApiTestRole + "-" + AlphaNumeric.CreateString(4),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();

                /*
                 * create random users
                 */
                user = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserV1()
                    {
                        UserName = AlphaNumeric.CreateString(4) + "-" + Constants.ApiTestUser,
                        PhoneNumber = Constants.ApiTestUserPhone + NumberAs.CreateString(1),
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }), Constants.ApiTestUserPassCurrent);

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activities>(new ActivityV1()
                    {
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_States>(new StateV1()
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
                    _mapper.Map<tbl_Refreshes>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = AlphaNumeric.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                /*
                 * set password for random audiences
                 */
                _uow.Audiences.SetPasswordHash(audience, Constants.ApiTestAudiencePassCurrent);
                _uow.Commit();

                /*
                 * assign roles to random audiences
                 */
                if (!_uow.Audiences.IsInRole(audience, role))
                    _uow.Audiences.AddToRole(audience, role);

                _uow.Commit();

                /*
                 * assign roles, claims & logins to random users
                 */
                _uow.Users.SetConfirmedEmail(user, true);
                _uow.Users.SetConfirmedPassword(user, true);
                _uow.Users.SetConfirmedPhoneNumber(user, true);
                _uow.Commit();

                if (!_uow.Users.IsInRole(user, role))
                    _uow.Users.AddToRole(user, role);

                if (!_uow.Users.IsInLogin(user, login))
                    _uow.Users.AddToLogin(user, login);

                if (!_uow.Users.IsInClaim(user, claim))
                    _uow.Users.AddToClaim(user, claim);

                _uow.Commit();
            }
        }

        public void CreateMOTD(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                _uow.MOTDs.Create(
                    new tbl_MOTDs()
                    {
                        Id = Guid.NewGuid(),
                        Author = Constants.ApiTestMOTD,
                        Quote = "Test Quote",
                        TssLength = 666,
                        TssId = AlphaNumeric.CreateString(8),
                        TssDate = DateTime.Now,
                        TssCategory = "Test Category",
                        TssTitle = "Test Title",
                        TssBackground = "Test Background",
                        TssTags = "tag1,tag2,tag3",
                    });

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test users
             */
            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName.Contains(Constants.ApiTestUser)).ToLambda());
            _uow.Commit();

            /*
             * delete test roles
             */
            _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Roles>()
                .Where(x => x.Name.Contains(Constants.ApiTestRole)).ToLambda());
            _uow.Commit();

            /*
             * delete test logins
             */
            _uow.Logins.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Logins>()
                .Where(x => x.Name.Contains(Constants.ApiTestLogin)).ToLambda());
            _uow.Commit();

            /*
             * delete test claims
             */
            _uow.Claims.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Claims>()
                .Where(x => x.Type.Contains(Constants.ApiTestClaim)).ToLambda());
            _uow.Commit();

            /*
             * delete test audiences
             */
            _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audiences>()
                .Where(x => x.Name.Contains(Constants.ApiTestAudience)).ToLambda());
            _uow.Commit();

            /*
             * delete test issuers
             */
            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Issuers>()
                .Where(x => x.Name.Contains(Constants.ApiTestIssuer)).ToLambda());
            _uow.Commit();

            /*
             * delete test msg of the day
             */
            _uow.MOTDs.Delete(QueryExpressionFactory.GetQueryExpression<tbl_MOTDs>()
                .Where(x => x.Author.Contains(Constants.ApiTestMOTD)).ToLambda());
            _uow.Commit();
        }
    }
}
