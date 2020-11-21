using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    public class TestDataFactory
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TestDataFactory(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public void Create()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test settings
             */
            var foundGlobalLegacyClaims = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            var foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            var foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalTotpExpire).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test issuers
             */
            var foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = Constants.TestIssuer,
                        IssuerKey = Constants.TestIssuerKey,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingAccessExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingRefreshExpire,
                        ConfigValue = 86400.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingTotpExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingPollingMax,
                        ConfigValue = 10.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test audiences
             */
            var foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.TestAudience,
                        IsLockedOut = false,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refresh>(new RefreshV1()
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
            var url = new Uri(Constants.TestUriLink);

            var foundAudienceUrl = _uow.Urls.Get(QueryExpressionFactory.GetQueryExpression<tbl_Url>()
                .Where(x => x.AudienceId == foundAudience.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath).ToLambda())
                .SingleOrDefault();

            if (foundAudienceUrl == null)
            {
                foundAudienceUrl = _uow.Urls.Create(
                    _mapper.Map<tbl_Url>(new UrlV1()
                    {
                        AudienceId = foundAudience.Id,
                        UrlHost = url.Scheme + "://" + url.Host,
                        UrlPath = url.AbsolutePath,
                        IsEnabled = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test claims
             */
            var foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == Constants.TestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _mapper.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.TestClaimSubject,
                        Type = Constants.TestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.TestClaimValueType,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test logins
             */
            var foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == Constants.TestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Login>(new LoginV1()
                    {
                        Name = Constants.TestLogin,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test roles
             */
            var foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == Constants.TestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.TestRole,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<tbl_User>(new UserV1()
                    {
                        UserName = Constants.TestUser,
                        Email = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = false,
                    }), Constants.TestUserPassCurrent);

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refresh>(new RefreshV1()
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
                    _mapper.Map<tbl_State>(new StateV1()
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
                    _mapper.Map<tbl_State>(new StateV1()
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
            _uow.Audiences.SetPasswordHash(foundAudience, Constants.TestAudiencePassCurrent);
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
                tbl_Issuer issuer;
                tbl_Audience audience;
                tbl_Url url;
                tbl_Role role;
                tbl_Login login;
                tbl_User user;
                tbl_Claim claim;

                /*
                 * create random issuers
                 */
                issuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = Constants.TestIssuer + "-" + Base64.CreateString(4),
                        IssuerKey = Constants.TestIssuerKey,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = issuer.Id,
                        ConfigKey = Constants.SettingAccessExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = issuer.Id,
                        ConfigKey = Constants.SettingRefreshExpire,
                        ConfigValue = 86400.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = issuer.Id,
                        ConfigKey = Constants.SettingTotpExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = issuer.Id,
                        ConfigKey = Constants.SettingPollingMax,
                        ConfigValue = 10.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();

                /*
                 * create random audiences
                 */
                audience = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = issuer.Id,
                        Name = Constants.TestAudience + "-" + Base64.CreateString(4),
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        AudienceId = audience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refresh>(new RefreshV1()
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
                var audienceUrl = new Uri(Constants.TestUriLink);

                url = _uow.Urls.Create(
                    _mapper.Map<tbl_Url>(new UrlV1()
                    {
                        AudienceId = audience.Id,
                        UrlHost = audienceUrl.Scheme + "://" + audienceUrl.Host,
                        UrlPath = audienceUrl.AbsolutePath,
                        IsEnabled = true,
                    }));

                _uow.Commit();

                /*
                 * create random claims
                 */
                claim = _uow.Claims.Create(
                    _mapper.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = issuer.Id,
                        Subject = Constants.TestClaimSubject + "-" + AlphaNumeric.CreateString(4),
                        Type = Constants.TestClaim + "-" + AlphaNumeric.CreateString(4),
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.TestClaimValueType + "-" + AlphaNumeric.CreateString(4),
                        IsDeletable = false,
                    }));

                _uow.Commit();

                /*
                 * create random logins
                 */
                login = _uow.Logins.Create(
                    _mapper.Map<tbl_Login>(new LoginV1()
                    {
                        Name = Constants.TestLogin + "-" + AlphaNumeric.CreateString(4),
                        LoginKey = Constants.TestLoginKey,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();

                /*
                 * create random roles
                 */
                role = _uow.Roles.Create(
                    _mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = audience.Id,
                        Name = Constants.TestRole + "-" + AlphaNumeric.CreateString(4),
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();

                /*
                 * create random users
                 */
                var userName = AlphaNumeric.CreateString(4);

                user = _uow.Users.Create(
                    _mapper.Map<tbl_User>(new UserV1()
                    {
                        UserName = userName + "-" + Constants.TestUser,
                        Email = userName + "-" + Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = false,
                    }), Constants.TestUserPassCurrent);

                _uow.Commit();

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        IsDeletable = false,
                    }));

                _uow.States.Create(
                    _mapper.Map<tbl_State>(new StateV1()
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
                    _mapper.Map<tbl_Refresh>(new RefreshV1()
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
                _uow.Audiences.SetPasswordHash(audience, Constants.TestAudiencePassCurrent);
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

        public void CreateEmail(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<tbl_User>(new UserV1()
                    {
                        UserName = Constants.TestUser,
                        Email = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = false,
                    }), Constants.TestUserPassCurrent);

                _uow.Commit();
            }

            for (int i = 0; i < sets; i++)
            {
                DateTime now = DateTime.UtcNow;

                var result = _uow.EmailQueue.Create(
                    new tbl_EmailQueue()
                    {
                        Id = Guid.NewGuid(),
                        FromId = foundUser.Id,
                        FromEmail = foundUser.EmailAddress,
                        ToId = foundUser.Id,
                        ToEmail = foundUser.EmailAddress,
                        Subject = "Subject-" + Base64.CreateString(4),
                        Body = "Body-" + Base64.CreateString(32),
                        CreatedUtc = now,
                        SendAtUtc = now,
                    });
            }
            _uow.Commit();
        }

        public void CreateText(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<tbl_User>(new UserV1()
                    {
                        UserName = Constants.TestUser,
                        Email = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = false,
                    }), Constants.TestUserPassCurrent);
                _uow.Commit();
            }

            for (int i = 0; i < sets; i++)
            {
                DateTime now = DateTime.UtcNow;

                var result = _uow.TextQueue.Create(
                    new tbl_TextQueue()
                    {
                        Id = Guid.NewGuid(),
                        FromId = foundUser.Id,
                        FromPhoneNumber = Constants.TestUserPhoneNumber,
                        ToId = foundUser.Id,
                        ToPhoneNumber = Constants.TestUserPhoneNumber,
                        Body = "Body-" + Base64.CreateString(32),
                        CreatedUtc = now,
                        SendAtUtc = now,
                    });
            }
            _uow.Commit();
        }

        public void CreateMOTD(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                _uow.MOTDs.Create(
                    new tbl_MOTD()
                    {
                        Id = Guid.NewGuid(),
                        Author = Constants.TestMotdAuthor,
                        Quote = "Quote-" + Base64.CreateString(4),
                        TssLength = 666,
                        TssId = AlphaNumeric.CreateString(8),
                        TssDate = DateTime.UtcNow,
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
             * delete test emails
             */
            _uow.EmailQueue.Delete(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda());
            _uow.Commit();

            /*
             * delete test texts
             */
            _uow.TextQueue.Delete(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ToLambda());
            _uow.Commit();

            /*
             * delete test motds
             */
            _uow.MOTDs.Delete(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .Where(x => x.Author.Contains(Constants.TestMotdAuthor)).ToLambda());
            _uow.Commit();

            /*
             * delete test users
             */
            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName.Contains(Constants.TestUser)).ToLambda());
            _uow.Commit();

            /*
             * delete test roles
             */
            _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name.Contains(Constants.TestRole)).ToLambda());
            _uow.Commit();

            /*
             * delete test logins
             */
            _uow.Logins.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name.Contains(Constants.TestLogin)).ToLambda());
            _uow.Commit();

            /*
             * delete test claims
             */
            _uow.Claims.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type.Contains(Constants.TestClaim)).ToLambda());
            _uow.Commit();

            /*
             * delete test audiences
             */
            _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name.Contains(Constants.TestAudience)).ToLambda());
            _uow.Commit();

            /*
             * delete test issuers
             */
            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name.Contains(Constants.TestIssuer)).ToLambda());
            _uow.Commit();

            /*
             * delete test msg of the day
             */
            _uow.MOTDs.Delete(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .Where(x => x.Author.Contains(Constants.TestMotdAuthor)).ToLambda());
            _uow.Commit();
        }
    }
}
