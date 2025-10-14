using AutoMapper;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public class DefaultDataFactory : IDefaultDataFactory
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _map;
        private readonly SeedDataSettings _seedData;

        private tbl_Issuer foundIssuer;
        private tbl_LoginProvider foundLoginProvider;
        private Dictionary<string, tbl_Audience> foundAudiences = new Dictionary<string, tbl_Audience>();
        private Dictionary<string, tbl_Role> foundRoles = new Dictionary<string, tbl_Role>();
        private Dictionary<string, tbl_User> foundUsers = new Dictionary<string, tbl_User>();
        private tbl_Setting foundGlobalLegacyClaims, foundGlobalLegacyIssuer, foundGlobalTotpExpire;

        public DefaultDataFactory(IUnitOfWork uow, SeedDataSettings seedData)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _seedData = seedData ?? throw new ArgumentNullException(nameof(seedData));
            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>())
                .CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            foreach (var audienceSeed in _seedData.Audiences)
            {
                var foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.Name == audienceSeed.Name).ToLambda())
                    .FirstOrDefault();

                if (foundAudience == null)
                {
                    foundAudience = _uow.Audiences.Create(
                        _map.Map<tbl_Audience>(new AudienceV1()
                        {
                            IssuerId = foundIssuer.Id,
                            Name = audienceSeed.Name,
                            IsLockedOut = false,
                            IsDeletable = false,
                        }));

                    _uow.Commit();
                }

                if (!_uow.Audiences.IsPasswordSet(foundAudience))
                {
                    _uow.Audiences.SetPassword(foundAudience, audienceSeed.Password);
                    _uow.Commit();
                }

                foundAudiences[audienceSeed.Name] = foundAudience;
            }
        }

        public void CreateAudienceRoles()
        {
            if (foundAudiences.Count == 0)
                CreateAudiences();

            if (foundRoles.Count == 0)
                CreateRoles();

            foreach (var audienceSeed in _seedData.Audiences)
            {
                if (!foundAudiences.TryGetValue(audienceSeed.Name, out var audience))
                    continue;

                var audienceRoles = _seedData.Roles
                    .Where(r => r.AudienceName == audienceSeed.Name)
                    .Where(r => r.Name.EndsWith(".Admins"))
                    .ToList();

                foreach (var roleSeed in audienceRoles)
                {
                    if (!foundRoles.TryGetValue(roleSeed.Name, out var role))
                        continue;

                    if (!_uow.Audiences.IsInRole(audience, role))
                    {
                        _uow.Audiences.AddRole(
                            new tbl_AudienceRole()
                            {
                                AudienceId = audience.Id,
                                RoleId = role.Id,
                                IsDeletable = true,
                                CreatedUtc = DateTime.UtcNow,
                            });

                        _uow.Commit();
                    }
                }
            }
        }

        public void CreateIssuers()
        {
            foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == _seedData.Issuer.Name).ToLambda())
                .FirstOrDefault();

            if (foundIssuer == null)
            {
                var issuerEntity = _map.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = _seedData.Issuer.Name,
                        IsEnabled = true,
                        IsDeletable = false,
                    });
                issuerEntity.IssuerKey = _seedData.Issuer.IssuerKey;

                foundIssuer = _uow.Issuers.Create(issuerEntity);

                _uow.Commit();
            }

            var foundAccessExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.AccessExpire).ToLambda())
                .FirstOrDefault();

            if (foundAccessExpire == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.AccessExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundRefreshExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.RefreshExpire).ToLambda())
                .FirstOrDefault();

            if (foundRefreshExpire == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.RefreshExpire,
                        ConfigValue = 86400.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundTotpExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.TotpExpire).ToLambda())
                .FirstOrDefault();

            if (foundTotpExpire == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.TotpExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundPollingMax = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.PollingMax).ToLambda())
                .FirstOrDefault();

            if (foundPollingMax == null)
            {
                _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.PollingMax,
                        ConfigValue = 10.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }
        }

        public void CreateJobs()
        {
            if (_seedData.Jobs == null)
                return;

            foreach (var jobSeed in _seedData.Jobs)
            {
                var foundJob = _uow.Jobs.Get(QueryExpressionFactory.GetQueryExpression<tbl_Job>()
                    .Where(x => x.Name == jobSeed.Name).ToLambda())
                    .FirstOrDefault();

                if (foundJob == null)
                {
                    foundJob = _uow.Jobs.Create(
                        new tbl_Job
                        {
                            Id = Guid.NewGuid(),
                            Name = jobSeed.Name,
                            IsEnabled = jobSeed.IsEnabled,
                            IsDeletable = false,
                            CreatedUtc = DateTimeOffset.UtcNow,
                        });

                    _uow.Commit();
                }

                if (jobSeed.Settings == null)
                    continue;

                foreach (var settingSeed in jobSeed.Settings)
                {
                    var foundSetting = _uow.JobSettings.Get(
                        x => x.JobId == foundJob.Id && x.ConfigKey == settingSeed.ConfigKey)
                        .FirstOrDefault();

                    if (foundSetting == null)
                    {
                        _uow.JobSettings.Create(
                            new tbl_JobSetting
                            {
                                Id = Guid.NewGuid(),
                                JobId = foundJob.Id,
                                ConfigKey = settingSeed.ConfigKey,
                                ConfigValue = settingSeed.ConfigValue,
                                IsSecret = settingSeed.IsSecret,
                                IsDeletable = false,
                                CreatedUtc = DateTimeOffset.UtcNow,
                            });

                        _uow.Commit();
                    }
                }
            }
        }

        public void CreateLLMProviders()
        {
            if (_seedData.LLMProviders == null)
                return;

            foreach (var providerSeed in _seedData.LLMProviders)
            {
                var foundProvider = _uow.LLMProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LLMProvider>()
                    .Where(x => x.Name == providerSeed.Name).ToLambda())
                    .FirstOrDefault();

                if (foundProvider == null)
                {
                    foundProvider = _uow.LLMProviders.Create(
                        new tbl_LLMProvider
                        {
                            Id = Guid.NewGuid(),
                            Name = providerSeed.Name,
                            IsEnabled = providerSeed.IsEnabled,
                            FailoverOrder = providerSeed.FailoverOrder,
                            IsDeletable = false,
                            CreatedUtc = DateTimeOffset.UtcNow,
                        });

                    _uow.Commit();
                }

                if (providerSeed.Settings == null)
                    continue;

                foreach (var settingSeed in providerSeed.Settings)
                {
                    var foundSetting = _uow.LLMProviderSettings.Get(
                        x => x.ProviderId == foundProvider.Id && x.ConfigKey == settingSeed.ConfigKey)
                        .FirstOrDefault();

                    if (foundSetting == null)
                    {
                        _uow.LLMProviderSettings.Create(
                            new tbl_LLMProviderSetting
                            {
                                Id = Guid.NewGuid(),
                                ProviderId = foundProvider.Id,
                                ConfigKey = settingSeed.ConfigKey,
                                ConfigValue = settingSeed.ConfigValue,
                                IsSecret = settingSeed.IsSecret,
                                IsDeletable = false,
                                CreatedUtc = DateTimeOffset.UtcNow,
                            });

                        _uow.Commit();
                    }
                }
            }
        }

        public void CreateLoginProviders()
        {
            foundLoginProvider = _uow.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Name == _seedData.LoginProvider.Name).ToLambda())
                .FirstOrDefault();

            if (foundLoginProvider == null)
            {
                foundLoginProvider = _uow.LoginProviders.Create(
                    _map.Map<tbl_LoginProvider>(new LoginProviderV1()
                    {
                        Name = _seedData.LoginProvider.Name,
                        ProviderKey = _seedData.LoginProvider.ProviderKey,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }
        }

        public void CreateRoles()
        {
            if (foundAudiences.Count == 0)
                CreateAudiences();

            foreach (var roleSeed in _seedData.Roles)
            {
                if (!foundAudiences.TryGetValue(roleSeed.AudienceName, out var audience))
                    continue;

                var foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                    .Where(x => x.Name == roleSeed.Name).ToLambda())
                    .FirstOrDefault();

                if (foundRole == null)
                {
                    foundRole = _uow.Roles.Create(
                        _map.Map<tbl_Role>(new RoleV1()
                        {
                            AudienceId = audience.Id,
                            Name = roleSeed.Name,
                            IsEnabled = true,
                            IsDeletable = false,
                        }));

                    _uow.Commit();
                }

                foundRoles[roleSeed.Name] = foundRole;
            }
        }

        public void CreateSettings()
        {
            foundGlobalLegacyClaims = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyClaims).FirstOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalLegacyClaims,
                        ConfigValue = "true",
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyIssuer).FirstOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalLegacyIssuer,
                        ConfigValue = "true",
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalTotpExpire).FirstOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
                    _map.Map<tbl_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }
        }

        public void CreateUsers()
        {
            foreach (var userSeed in _seedData.Users)
            {
                var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.UserName == userSeed.UserName).ToLambda())
                    .FirstOrDefault();

                if (foundUser == null)
                {
                    foundUser = _uow.Users.Create(
                        _map.Map<tbl_User>(new UserV1()
                        {
                            UserName = userSeed.UserName,
                            Email = userSeed.UserName,
                            FirstName = userSeed.FirstName,
                            LastName = userSeed.LastName,
                            IsHumanBeing = true,
                            IsLockedOut = false,
                            IsDeletable = false,
                        }), userSeed.Password);

                    _uow.Users.SetConfirmedEmail(foundUser, true);
                    _uow.Users.SetConfirmedPhoneNumber(foundUser, true);
                    _uow.Users.SetConfirmedPassword(foundUser, true);

                    _uow.Commit();
                }

                foundUsers[userSeed.UserName] = foundUser;
            }
        }

        public void CreateUserLoginProviders()
        {
            if (foundUsers.Count == 0)
                CreateUsers();

            if (foundLoginProvider == null)
                CreateLoginProviders();

            foreach (var userSeed in _seedData.Users)
            {
                if (!foundUsers.TryGetValue(userSeed.UserName, out var user))
                    continue;

                if (!_uow.Users.IsInLoginProvider(user, foundLoginProvider))
                {
                    _uow.Users.AddLoginProvider(
                        new tbl_UserLoginProvider()
                        {
                            UserId = user.Id,
                            LoginProviderId = foundLoginProvider.Id,
                            IsDeletable = true,
                            CreatedUtc = DateTime.UtcNow,
                        });

                    _uow.Commit();
                }
            }
        }

        public void CreateUserRoles()
        {
            if (foundUsers.Count == 0)
                CreateUsers();

            if (foundRoles.Count == 0)
                CreateRoles();

            foreach (var userSeed in _seedData.Users)
            {
                if (!foundUsers.TryGetValue(userSeed.UserName, out var user))
                    continue;

                if (userSeed.Roles == null)
                    continue;

                foreach (var roleName in userSeed.Roles)
                {
                    if (!foundRoles.TryGetValue(roleName, out var role))
                        continue;

                    if (!_uow.Users.IsInRole(user, role))
                    {
                        _uow.Users.AddRole(
                            new tbl_UserRole()
                            {
                                UserId = user.Id,
                                RoleId = role.Id,
                                IsDeletable = true,
                                CreatedUtc = DateTime.UtcNow,
                            });

                        _uow.Commit();
                    }
                }
            }
        }

        public void Destroy()
        {
            foreach (var userSeed in _seedData.Users)
            {
                _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.UserName == userSeed.UserName).ToLambda());
            }
            _uow.Commit();

            foreach (var roleSeed in _seedData.Roles)
            {
                _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                    .Where(x => x.Name == roleSeed.Name).ToLambda());
            }
            _uow.Commit();

            _uow.LoginProviders.Delete(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Name == _seedData.LoginProvider.Name).ToLambda());
            _uow.Commit();

            foreach (var audienceSeed in _seedData.Audiences)
            {
                _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.Name == audienceSeed.Name).ToLambda());
            }
            _uow.Commit();

            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == _seedData.Issuer.Name).ToLambda());
            _uow.Commit();
        }
    }
}
