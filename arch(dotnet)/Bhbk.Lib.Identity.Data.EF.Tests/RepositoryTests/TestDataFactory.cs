using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests
{
    public class TestDataFactory : IDisposable
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _map;
        private readonly TestDataSettings _testData;
        private tbl_Setting foundGlobalLegacyClaims, foundGlobalLegacyIssuer, foundGlobalTotpExpire;
        private tbl_Issuer foundIssuer;
        private tbl_Audience foundAudience;
        private tbl_Url foundAudienceUrl;
        private tbl_LoginProvider foundLoginProvider;
        private tbl_Claim foundClaim;
        private tbl_Role foundRole;
        private tbl_User foundUser;
        private bool disposedValue;

        public TestDataFactory(IUnitOfWork uow, TestDataSettings testData)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _testData = testData ?? throw new ArgumentNullException();

            if (_uow.InstanceType == InstanceContext.DeployedOrLocal
                || _uow.InstanceType == InstanceContext.End2EndTest)
                throw new InvalidOperationException();

            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create test audiences
             */

            foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == _testData.Audience.Name).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _map.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = _testData.Audience.Name,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                var activity = _uow.AuthActivity.Create(
                    _map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                    }));

                _uow.AuthActivityAudiences.Create(new tbl_AuthActivityAudience
                {
                    AuthActivityId = activity.Id,
                    AudienceId = foundAudience.Id,
                    CreatedUtc = activity.CreatedUtc,
                });

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            if (!_uow.Audiences.IsPasswordSet(foundAudience))
            {
                _uow.Audiences.SetPassword(foundAudience, _testData.Audience.PasswordCurrent);
                _uow.Commit();
            }
        }

        public void CreateAudienceRefreshes()
        {
            if (foundIssuer == null)
                CreateIssuers();

            if (foundAudience == null)
                CreateAudiences();

            /*
             * create test refreshes
             */

            _uow.Refreshes.Create(
                _map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    RefreshType = ConsumerType.Client.ToString(),
                    RefreshValue = AlphaNumeric.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.Commit();
        }

        public void CreateAudienceRoles()
        {
            if (foundAudience == null)
                CreateAudiences();

            if (foundRole == null)
                CreateRoles();

            /*
             * assign roles to audiences
             */

            if (!_uow.Audiences.IsInRole(foundAudience, foundRole))
            {
                _uow.Audiences.AddRole(
                    new tbl_AudienceRole()
                    {
                        AudienceId = foundAudience.Id,
                        RoleId = foundRole.Id,
                        IsDeletable = true,
                        CreatedUtc = DateTime.UtcNow,
                    });

                _uow.Commit();
            }
        }

        public void CreateClaims()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create test claims
             */

            foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == _testData.Claim.Name).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _map.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = _testData.Claim.Subject,
                        Type = _testData.Claim.Name,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = _testData.Claim.ValueType,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateEmails()
        {
            if (foundUser == null)
                CreateUsers();

            var sets = 2;

            for (int i = 0; i < sets; i++)
            {
                var now = DateTime.UtcNow;

                _uow.EmailQueue.Create(
                    _map.Map<tbl_EmailQueue>(new EmailV1()
                    {
                        FromEmail = foundUser.EmailAddress,
                        ToEmail = foundUser.EmailAddress,
                        Subject = "Subject-" + Base64.CreateString(4),
                        Body = "Body-" + Base64.CreateString(32),
                        CreatedUtc = now,
                        SendAtUtc = now,
                    }));
            }

            _uow.Commit();
        }

        public void CreateIssuers()
        {
            /*
             * create test issuers
             */

            foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == _testData.Issuer.Name).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                var issuerEntity = _map.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = _testData.Issuer.Name,
                        IsEnabled = true,
                        IsDeletable = true,
                    });
                issuerEntity.IssuerKey = _testData.Issuer.IssuerKey;

                foundIssuer = _uow.Issuers.Create(issuerEntity);

                _uow.Commit();
            }

            var foundAccessExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.AccessExpire).ToLambda())
                .SingleOrDefault();

            if (foundAccessExpire == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.AccessExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            var foundRefreshExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.RefreshExpire).ToLambda())
                .SingleOrDefault();

            if (foundRefreshExpire == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.RefreshExpire,
                        ConfigValue = 86400.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            var foundTotpExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.TotpExpire).ToLambda())
                .SingleOrDefault();

            if (foundTotpExpire == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.TotpExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            var foundPollingMax = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.PollingMax).ToLambda())
                .SingleOrDefault();

            if (foundPollingMax == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.PollingMax,
                        ConfigValue = 10.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateLoginProviders()
        {
            /*
             * create test login providers
             */

            foundLoginProvider = _uow.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Name == _testData.LoginProvider.Name).ToLambda())
                .SingleOrDefault();

            if (foundLoginProvider == null)
            {
                foundLoginProvider = _uow.LoginProviders.Create(
                    _map.Map<tbl_LoginProvider>(new LoginProviderV1()
                    {
                        Name = _testData.LoginProvider.Name,
                        ProviderKey = AlphaNumeric.CreateString(16),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateQuotes()
        {
            var sets = 2;

            for (int i = 0; i < sets; i++)
            {
                _uow.Quotes.Create(
                    _map.Map<tbl_Quote>(new QuoteV1()
                    {
                        globalId = Guid.NewGuid(),
                        author = _testData.Quote.Author,
                        quote = "Quote-" + Base64.CreateString(4),
                        length = 666.ToString(),
                        id = AlphaNumeric.CreateString(8),
                        date = DateTime.UtcNow.ToString(),
                        category = "Test Category",
                        title = "Test Title",
                        background = "Test Background",
                        tags = "tag1,tag2,tag3".Split(',', StringSplitOptions.None).ToList(),
                    }));
            }

            _uow.Commit();
        }

        public void CreateRoles()
        {
            if (foundAudience == null)
                CreateAudiences();

            /*
             * create test roles
             */

            foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == _testData.Role.Name).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _map.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = _testData.Role.Name,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateSettings()
        {
            /*
             * create test settings
             */

            foundGlobalLegacyClaims = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalLegacyClaims,
                        ConfigValue = "true",
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyIssuer).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalLegacyIssuer,
                        ConfigValue = "true",
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalTotpExpire).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateTexts()
        {
            if (foundUser == null)
                CreateUsers();

            var sets = 2;

            for (int i = 0; i < sets; i++)
            {
                var now = DateTime.UtcNow;

                _uow.TextQueue.Create(
                    _map.Map<tbl_TextQueue>(new TextV1()
                    {
                        FromPhoneNumber = _testData.User.PhoneNumber,
                        ToPhoneNumber = _testData.User.PhoneNumber,
                        Body = "Body-" + Base64.CreateString(32),
                        CreatedUtc = now,
                        SendAtUtc = now,
                    }));
            }

            _uow.Commit();
        }

        public void CreateUrls()
        {
            if (foundAudience == null)
                CreateAudiences();

            /*
             * create test client urls
             */

            var url = new Uri(_testData.Url.Link);

            foundAudienceUrl = _uow.Urls.Get(QueryExpressionFactory.GetQueryExpression<tbl_Url>()
                .Where(x => x.AudienceId == foundAudience.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath).ToLambda())
                .SingleOrDefault();

            if (foundAudienceUrl == null)
            {
                foundAudienceUrl = _uow.Urls.Create(
                    _map.Map<tbl_Url>(new UrlV1()
                    {
                        AudienceId = foundAudience.Id,
                        UrlHost = url.Scheme + "://" + url.Host,
                        UrlPath = url.AbsolutePath,
                        IsEnabled = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateUsers()
        {
            /*
             * create test users
             */

            foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == _testData.User.UserName).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _map.Map<tbl_User>(new UserV1()
                    {
                        UserName = _testData.User.UserName,
                        Email = _testData.User.UserName,
                        PhoneNumber = NumberAs.CreateString(11),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }), _testData.User.PasswordCurrent);

                _uow.Commit();

                _uow.AuthActivity.Create(
                    _map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        UserId = foundUser.Id,
                        LoginType = GrantFlowType.ResourceOwnerPasswordV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                    }));

                _uow.Users.SetConfirmedEmail(foundUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundUser, true);
                _uow.Users.SetConfirmedPassword(foundUser, true);

                _uow.Commit();
            }
        }

        public void CreateUserClaims()
        {
            if (foundUser == null)
                CreateUsers();

            if (foundClaim == null)
                CreateClaims();

            /*
             * assign claim to users
             */

            if (!_uow.Users.IsInClaim(foundUser, foundClaim))
            {
                _uow.Users.AddClaim(
                    new tbl_UserClaim()
                    {
                        UserId = foundUser.Id,
                        ClaimId = foundClaim.Id,
                        IsDeletable = true,
                        CreatedUtc = DateTime.UtcNow,
                    });

                _uow.Commit();
            }
        }

        public void CreateUserLoginProviders()
        {
            if (foundUser == null)
                CreateUsers();

            if (foundLoginProvider == null)
                CreateLoginProviders();

            /*
             * assign login provider to users
             */

            if (!_uow.Users.IsInLoginProvider(foundUser, foundLoginProvider))
            {
                _uow.Users.AddLoginProvider(
                    new tbl_UserLoginProvider()
                    {
                        UserId = foundUser.Id,
                        LoginProviderId = foundLoginProvider.Id,
                        IsDeletable = true,
                        CreatedUtc = DateTime.UtcNow,
                    });

                _uow.Commit();
            }
        }

        public void CreateUserRefreshes()
        {
            if (foundIssuer == null)
                CreateIssuers();

            if (foundAudience == null)
                CreateAudiences();

            if (foundUser == null)
                CreateUsers();

            /*
             * create test refreshes
             */

            _uow.Refreshes.Create(
                _map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = AlphaNumeric.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.Commit();
        }

        public void CreateUserRoles()
        {
            if (foundUser == null)
                CreateUsers();

            if (foundRole == null)
                CreateRoles();

            /*
             * assign roles to users
             */

            if (!_uow.Users.IsInRole(foundUser, foundRole))
            {
                _uow.Users.AddRole(
                    new tbl_UserRole()
                    {
                        UserId = foundUser.Id,
                        RoleId = foundRole.Id,
                        IsDeletable = true,
                        CreatedUtc = DateTime.UtcNow,
                    });

                _uow.Commit();
            }
        }

        public void CreateUserStates()
        {
            if (foundIssuer == null)
                CreateIssuers();

            if (foundAudience == null)
                CreateAudiences();

            if (foundUser == null)
                CreateUsers();

            /*
             * create test states
             */

            _uow.States.Create(
                _map.Map<tbl_State>(new StateV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = true,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.States.Create(
                _map.Map<tbl_State>(new StateV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.Commit();
        }

        public void Destroy()
        {
            /*
             * delete test quotes
             */

            var quotes = _uow.Quotes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                .Where(x => x.Author.Contains(_testData.Quote.Author)).ToLambda());

            if (quotes.Count() > 0)
            {
                _uow.Quotes.Delete(quotes);
                _uow.Commit();
            }

            /*
             * delete test emails
             */

            var emails = _uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda());

            if (emails.Count() > 0)
            {
                _uow.EmailQueue.Delete(emails);
                _uow.Commit();
            }

            /*
             * delete test texts
             */

            var texts = _uow.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ToLambda());

            if (texts.Count() > 0)
            {
                _uow.TextQueue.Delete(texts);
                _uow.Commit();
            }

            /*
             * delete test users
             */

            var users = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName.Contains(_testData.User.UserName)).ToLambda());

            if (users.Count() > 0)
            {
                _uow.Users.Delete(users);
                _uow.Commit();
            }

            /*
             * delete test roles
             */

            var roles = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name.Contains(_testData.Role.Name)).ToLambda());

            if (roles.Count() > 0)
            {
                _uow.Roles.Delete(roles);
                _uow.Commit();
            }

            /*
             * delete test login providers
             */

            var loginProviders = _uow.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Name.Contains(_testData.LoginProvider.Name)).ToLambda());

            if (loginProviders.Count() > 0)
            {
                _uow.LoginProviders.Delete(loginProviders);
                _uow.Commit();
            }

            /*
             * delete test claims
             */

            var claims = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type.Contains(_testData.Claim.Name)).ToLambda());

            if (claims.Count() > 0)
            {
                _uow.Claims.Delete(claims);
                _uow.Commit();
            }

            /*
             * delete test audiences
             */

            var audiences = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name.Contains(_testData.Audience.Name)).ToLambda());

            if (audiences.Count() > 0)
            {
                _uow.Audiences.Delete(audiences);
                _uow.Commit();
            }

            /*
             * delete test issuers
             */

            var issuers = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name.Contains(_testData.Issuer.Name)).ToLambda());

            if (issuers.Count() > 0)
            {
                _uow.Issuers.Delete(issuers);
                _uow.Commit();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Destroy();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
