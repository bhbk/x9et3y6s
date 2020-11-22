using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TSQL;
using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_TSQL
{
    public class TestDataFactory_TSQL : IDisposable
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private uvw_Setting foundGlobalLegacyClaims, foundGlobalLegacyIssuer, foundGlobalTotpExpire;
        private uvw_Issuer foundIssuer;
        private uvw_Audience foundAudience;
        private uvw_Url foundAudienceUrl;
        private uvw_Login foundLogin;
        private uvw_Claim foundClaim;
        private uvw_Role foundRole;
        private uvw_User foundUser;
        private bool disposedValue;

        public TestDataFactory_TSQL(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();

            if (_uow.InstanceType == InstanceContext.DeployedOrLocal
                || _uow.InstanceType == InstanceContext.End2EndTest)
                throw new InvalidOperationException();

            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TSQL>()).CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create test audiences
             */

            foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _mapper.Map<uvw_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.TestAudience,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.Activities.Create(
                    _mapper.Map<uvw_Activity>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            if (!_uow.Audiences.IsPasswordSet(foundAudience))
            {
                _uow.Audiences.SetPassword(foundAudience, Constants.TestAudiencePassCurrent);
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
                _mapper.Map<uvw_Refresh>(new RefreshV1()
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
                    new uvw_AudienceRole()
                    {
                        AudienceId = foundAudience.Id,
                        RoleId = foundRole.Id,
                        IsDeletable = true,
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

            foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claim>()
                .Where(x => x.Type == Constants.TestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _mapper.Map<uvw_Claim>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.TestClaimSubject,
                        Type = Constants.TestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.TestClaimValueType,
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
                    _mapper.Map<uvw_EmailQueue>(new EmailV1()
                    {
                        FromId = foundUser.Id,
                        FromEmail = foundUser.EmailAddress,
                        ToId = foundUser.Id,
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

            foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<uvw_Issuer>(new IssuerV1()
                    {
                        Name = Constants.TestIssuer,
                        IssuerKey = Constants.TestIssuerKey,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingAccessExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingRefreshExpire,
                        ConfigValue = 86400.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingTotpExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.SettingPollingMax,
                        ConfigValue = 10.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateLogins()
        {
            /*
             * create test logins
             */

            foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == Constants.TestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<uvw_Login>(new LoginV1()
                    {
                        Name = Constants.TestLogin,
                        LoginKey = AlphaNumeric.CreateString(16),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateMOTDs()
        {
            var sets = 2;

            for (int i = 0; i < sets; i++)
            {
                _uow.MOTDs.Create(
                    _mapper.Map<uvw_MOTD>(new MOTDTssV1()
                    {
                        globalId = Guid.NewGuid(),
                        author = Constants.TestMotdAuthor,
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

            foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.TestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.TestRole,
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
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalTotpExpire).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalTotpExpire,
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
                    _mapper.Map<uvw_TextQueue>(new TextV1()
                    {
                        FromId = foundUser.Id,
                        FromPhoneNumber = Constants.TestUserPhoneNumber,
                        ToId = foundUser.Id,
                        ToPhoneNumber = Constants.TestUserPhoneNumber,
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

            var url = new Uri(Constants.TestUriLink);

            foundAudienceUrl = _uow.Urls.Get(QueryExpressionFactory.GetQueryExpression<uvw_Url>()
                .Where(x => x.AudienceId == foundAudience.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath).ToLambda())
                .SingleOrDefault();

            if (foundAudienceUrl == null)
            {
                foundAudienceUrl = _uow.Urls.Create(
                    _mapper.Map<uvw_Url>(new UrlV1()
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

            foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<uvw_User>(new UserV1()
                    {
                        UserName = Constants.TestUser,
                        Email = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(11),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }), Constants.TestUserPassCurrent);

                _uow.Commit();

                _uow.Activities.Create(
                    _mapper.Map<uvw_Activity>(new ActivityV1()
                    {
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        IsDeletable = true,
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
                    new uvw_UserClaim()
                    {
                        UserId = foundUser.Id,
                        ClaimId = foundClaim.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }
        }

        public void CreateUserLogins()
        {
            if (foundUser == null)
                CreateUsers();

            if (foundLogin == null)
                CreateLogins();

            /*
             * assign login to users
             */

            if (!_uow.Users.IsInLogin(foundUser, foundLogin))
            {
                _uow.Users.AddLogin(
                        new uvw_UserLogin()
                        {
                            UserId = foundUser.Id,
                            LoginId = foundLogin.Id,
                            IsDeletable = true,
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
                _mapper.Map<uvw_Refresh>(new RefreshV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    RefreshType = RefreshType.User.ToString(),
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
                    new uvw_UserRole()
                    {
                        UserId = foundUser.Id,
                        RoleId = foundRole.Id,
                        IsDeletable = true,
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
                _mapper.Map<uvw_State>(new StateV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = true,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.States.Create(
                _mapper.Map<uvw_State>(new StateV1()
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

            _uow.Commit();
        }

        public void Destroy()
        {
            /*
             * delete test motds
             */

            var motds = _uow.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<uvw_MOTD>()
                .Where(x => x.Author.Contains(Constants.TestMotdAuthor)).ToLambda());

            if (motds.Count() > 0)
            {
                _uow.MOTDs.Delete(motds);
                _uow.Commit();
            }

            /*
             * delete test emails
             */

            var emails = _uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_EmailQueue>().ToLambda());

            if (emails.Count() > 0)
            {
                _uow.EmailQueue.Delete(emails);
                _uow.Commit();
            }

            /*
             * delete test texts
             */

            var texts = _uow.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>().ToLambda());

            if (texts.Count() > 0)
            {
                _uow.TextQueue.Delete(texts);
                _uow.Commit();
            }

            /*
             * delete test users
             */

            var users = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName.Contains(Constants.TestUser)).ToLambda());

            if (users.Count() > 0)
            {
                _uow.Users.Delete(users);
                _uow.Commit();
            }

            /*
             * delete test roles
             */

            var roles = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name.Contains(Constants.TestRole)).ToLambda());

            if (roles.Count() > 0)
            {
                _uow.Roles.Delete(roles);
                _uow.Commit();
            }

            /*
             * delete test logins
             */

            var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name.Contains(Constants.TestLogin)).ToLambda());

            if (logins.Count() > 0)
            {
                _uow.Logins.Delete(logins);
                _uow.Commit();
            }

            /*
             * delete test claims
             */

            var claims = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claim>()
                .Where(x => x.Type.Contains(Constants.TestClaim)).ToLambda());

            if (claims.Count() > 0)
            {
                _uow.Claims.Delete(claims);
                _uow.Commit();
            }

            /*
             * delete test audiences
             */

            var audiences = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name.Contains(Constants.TestAudience)).ToLambda());

            if (audiences.Count() > 0)
            {
                _uow.Audiences.Delete(audiences);
                _uow.Commit();
            }

            /*
             * delete test issuers
             */

            var issuers = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name.Contains(Constants.TestIssuer)).ToLambda());

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
                {
                    Destroy();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TestDataFactory_TSQL()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
