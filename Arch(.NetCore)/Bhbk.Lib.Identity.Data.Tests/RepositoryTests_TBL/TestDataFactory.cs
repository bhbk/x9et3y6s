﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl
{
    public class TestDataFactory : IDisposable
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _map;
        private tbl_Setting foundGlobalLegacyClaims, foundGlobalLegacyIssuer, foundGlobalTotpExpire;
        private tbl_Issuer foundIssuer;
        private tbl_Audience foundAudience;
        private tbl_Url foundAudienceUrl;
        private tbl_Login foundLogin;
        private tbl_Claim foundClaim;
        private tbl_Role foundRole;
        private tbl_User foundUser;
        private bool disposedValue;

        public TestDataFactory(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();

            if (_uow.InstanceType == InstanceContext.DeployedOrLocal
                || _uow.InstanceType == InstanceContext.End2EndTest)
                throw new InvalidOperationException();

            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TBL>()).CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create test audiences
             */

            foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _map.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = TestDefaultConstants.AudienceName,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.AuthActivity.Create(
                    _map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                    }));

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            if (!_uow.Audiences.IsPasswordSet(foundAudience))
            {
                _uow.Audiences.SetPassword(foundAudience, TestDefaultConstants.AudiencePassCurrent);
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
                .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _map.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = TestDefaultConstants.ClaimSubject,
                        Type = TestDefaultConstants.ClaimName,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = TestDefaultConstants.ClaimValueType,
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
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _map.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = TestDefaultConstants.IssuerName,
                        IssuerKey = TestDefaultConstants.IssuerKey,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

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

        public void CreateLogins()
        {
            /*
             * create test logins
             */

            foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == TestDefaultConstants.LoginName).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _map.Map<tbl_Login>(new LoginV1()
                    {
                        Name = TestDefaultConstants.LoginName,
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
                    _map.Map<tbl_MOTD>(new MOTDTssV1()
                    {
                        globalId = Guid.NewGuid(),
                        author = TestDefaultConstants.MOTDAuthor,
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
                .Where(x => x.Name == TestDefaultConstants.RoleName).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _map.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = TestDefaultConstants.RoleName,
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
                        FromPhoneNumber = TestDefaultConstants.UserPhoneNumber,
                        ToPhoneNumber = TestDefaultConstants.UserPhoneNumber,
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

            var url = new Uri(TestDefaultConstants.UriLink);

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
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _map.Map<tbl_User>(new UserV1()
                    {
                        UserName = TestDefaultConstants.UserName,
                        Email = TestDefaultConstants.UserName,
                        PhoneNumber = NumberAs.CreateString(11),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }), TestDefaultConstants.UserPassCurrent);

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
                    new tbl_UserLogin()
                    {
                        UserId = foundUser.Id,
                        LoginId = foundLogin.Id,
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
             * delete test motds
             */

            var motds = _uow.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .Where(x => x.Author.Contains(TestDefaultConstants.MOTDAuthor)).ToLambda());

            if (motds.Count() > 0)
            {
                _uow.MOTDs.Delete(motds);
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
                .Where(x => x.UserName.Contains(TestDefaultConstants.UserName)).ToLambda());

            if (users.Count() > 0)
            {
                _uow.Users.Delete(users);
                _uow.Commit();
            }

            /*
             * delete test roles
             */

            var roles = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name.Contains(TestDefaultConstants.RoleName)).ToLambda());

            if (roles.Count() > 0)
            {
                _uow.Roles.Delete(roles);
                _uow.Commit();
            }

            /*
             * delete test logins
             */

            var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name.Contains(TestDefaultConstants.LoginName)).ToLambda());

            if (logins.Count() > 0)
            {
                _uow.Logins.Delete(logins);
                _uow.Commit();
            }

            /*
             * delete test claims
             */

            var claims = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type.Contains(TestDefaultConstants.ClaimName)).ToLambda());

            if (claims.Count() > 0)
            {
                _uow.Claims.Delete(claims);
                _uow.Commit();
            }

            /*
             * delete test audiences
             */

            var audiences = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name.Contains(TestDefaultConstants.AudienceName)).ToLambda());

            if (audiences.Count() > 0)
            {
                _uow.Audiences.Delete(audiences);
                _uow.Commit();
            }

            /*
             * delete test issuers
             */

            var issuers = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name.Contains(TestDefaultConstants.IssuerName)).ToLambda());

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
        // ~TestDataFactory_TBL()
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
