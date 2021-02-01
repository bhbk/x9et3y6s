using AutoMapper;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public class DefaultDataFactory : IDefaultDataFactory
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private uvw_Issuer foundIssuer;
        private uvw_Audience foundAudience_Alert, foundAudience_Identity;
        private uvw_Login foundLogin;
        private uvw_Role foundRoleForAdmin_Alert, foundRoleForAdmin_Identity, foundRoleForUser_Alert, foundRoleForUser_Identity;
        private uvw_User foundAdmin, foundUser;
        private uvw_Setting foundGlobalLegacyClaims, foundGlobalLegacyIssuer, foundGlobalTotpExpire;

        public DefaultDataFactory(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create default audiences
             */

            foundAudience_Alert = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == DefaultConstants.Audience_Alert).ToLambda())
                .SingleOrDefault();

            if (foundAudience_Alert == null)
            {
                foundAudience_Alert = _uow.Audiences.Create(
                    _mapper.Map<uvw_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = DefaultConstants.Audience_Alert,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundAudience_Identity = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == DefaultConstants.Audience_Identity).ToLambda())
                .SingleOrDefault();

            if (foundAudience_Identity == null)
            {
                foundAudience_Identity = _uow.Audiences.Create(
                    _mapper.Map<uvw_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = DefaultConstants.Audience_Identity,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            if (!_uow.Audiences.IsPasswordSet(foundAudience_Alert))
            {
                _uow.Audiences.SetPassword(foundAudience_Alert, DefaultConstants.AudiencePassword_Alert);
                _uow.Commit();
            }

            if (!_uow.Audiences.IsPasswordSet(foundAudience_Identity))
            {
                _uow.Audiences.SetPassword(foundAudience_Identity, DefaultConstants.AudiencePassword_Identity);
                _uow.Commit();
            }
        }

        public void CreateAudienceRoles()
        {
            if (foundAudience_Alert == null
                || foundAudience_Identity == null)
                CreateAudiences();

            if (foundRoleForAdmin_Alert == null
                || foundRoleForAdmin_Identity == null
                || foundRoleForUser_Alert == null
                || foundRoleForUser_Identity == null)
                CreateRoles();

            /*
             * assign roles to audiences
             */

            if (!_uow.Audiences.IsInRole(foundAudience_Alert, foundRoleForAdmin_Alert))
            {
                _uow.Audiences.AddRole(
                    new uvw_AudienceRole()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        RoleId = foundRoleForAdmin_Alert.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }

            if (!_uow.Audiences.IsInRole(foundAudience_Identity, foundRoleForAdmin_Identity))
            {
                _uow.Audiences.AddRole(
                    new uvw_AudienceRole()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        RoleId = foundRoleForAdmin_Identity.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }
        }

        public void CreateIssuers()
        {
            /*
             * create default issuers
             */

            foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == DefaultConstants.IssuerName).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<uvw_Issuer>(new IssuerV1()
                    {
                        Name = DefaultConstants.IssuerName,
                        IssuerKey = DefaultConstants.IssuerKey,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundAccessExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<uvw_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.AccessExpire).ToLambda())
                .SingleOrDefault();

            if (foundAccessExpire == null)
            {
                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.AccessExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundRefreshExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<uvw_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.RefreshExpire).ToLambda())
                .SingleOrDefault();

            if (foundRefreshExpire == null)
            {
                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.RefreshExpire,
                        ConfigValue = 86400.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundTotpExpire = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<uvw_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.TotpExpire).ToLambda())
                .SingleOrDefault();

            if (foundTotpExpire == null)
            {
                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.TotpExpire,
                        ConfigValue = 600.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            var foundPollingMax = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<uvw_Setting>()
                .Where(x => x.IssuerId == foundIssuer.Id && x.ConfigKey == SettingsConstants.PollingMax).ToLambda())
                .SingleOrDefault();

            if (foundPollingMax == null)
            {
                _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = SettingsConstants.PollingMax,
                        ConfigValue = 10.ToString(),
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }
        }

        public void CreateLogins()
        {
            /*
             * create default logins
             */

            foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == DefaultConstants.LoginName).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<uvw_Login>(new LoginV1()
                    {
                        Name = DefaultConstants.LoginName,
                        LoginKey = DefaultConstants.LoginKey,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }
        }

        public void CreateRoles()
        {
            if (foundAudience_Alert == null
                || foundAudience_Identity == null)
                CreateAudiences();

            /*
             * create default roles
             */

            foundRoleForAdmin_Alert = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == DefaultConstants.RoleForAdmins_Alert).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin_Alert == null)
            {
                foundRoleForAdmin_Alert = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        Name = DefaultConstants.RoleForAdmins_Alert,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundRoleForUser_Alert = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == DefaultConstants.RoleForUsers_Alert).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser_Alert == null)
            {
                foundRoleForUser_Alert = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        Name = DefaultConstants.RoleForUsers_Alert,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundRoleForAdmin_Identity = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == DefaultConstants.RoleForAdmins_Identity).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin_Identity == null)
            {
                foundRoleForAdmin_Identity = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        Name = DefaultConstants.RoleForAdmins_Identity,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundRoleForUser_Identity = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == DefaultConstants.RoleForUsers_Identity).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser_Identity == null)
            {
                foundRoleForUser_Identity = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        Name = DefaultConstants.RoleForUsers_Identity,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }
        }

        public void CreateSettings()
        {
            /*
             * create default settings
             */

            foundGlobalLegacyClaims = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalLegacyClaims,
                        ConfigValue = "true",
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundGlobalLegacyIssuer = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyIssuer).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        ConfigKey = SettingsConstants.GlobalLegacyIssuer,
                        ConfigValue = "true",
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundGlobalTotpExpire = _uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalTotpExpire).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
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
            /*
             * create default users
             */

            foundAdmin = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == DefaultConstants.UserName_Admin).ToLambda())
                .SingleOrDefault();

            if (foundAdmin == null)
            {
                foundAdmin = _uow.Users.Create(
                    _mapper.Map<uvw_User>(new UserV1()
                    {
                        UserName = DefaultConstants.UserName_Admin,
                        Email = DefaultConstants.UserName_Admin,
                        FirstName = DefaultConstants.UserFirstName_Admin,
                        LastName = DefaultConstants.UserLastName_Admin,
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }), DefaultConstants.UserPass_Admin);

                _uow.Users.SetConfirmedEmail(foundAdmin, true);
                _uow.Users.SetConfirmedPhoneNumber(foundAdmin, true);
                _uow.Users.SetConfirmedPassword(foundAdmin, true);

                _uow.Commit();
            }

            foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == DefaultConstants.UserName_Normal).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<uvw_User>(new UserV1()
                    {
                        UserName = DefaultConstants.UserName_Normal,
                        Email = DefaultConstants.UserName_Normal,
                        FirstName = DefaultConstants.UserFirstName_Normal,
                        LastName = DefaultConstants.UserLastName_Normal,
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }), DefaultConstants.UserPass_Normal);

                _uow.Users.SetConfirmedEmail(foundUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundUser, true);
                _uow.Users.SetConfirmedPassword(foundUser, true);

                _uow.Commit();
            }
        }

        public void CreateUserLogins()
        {
            if (foundAdmin == null
                || foundUser == null)
                CreateUsers();

            if (foundLogin == null)
                CreateLogins();

            /*
             * assign logins to users
             */

            if (!_uow.Users.IsInLogin(foundAdmin, foundLogin))
            {
                _uow.Users.AddLogin(
                    new uvw_UserLogin()
                    {
                        UserId = foundAdmin.Id,
                        LoginId = foundLogin.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }

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

        public void CreateUserRoles()
        {
            if (foundAdmin == null
                || foundUser == null)
                CreateUsers();

            if (foundRoleForAdmin_Alert == null
                || foundRoleForAdmin_Identity == null
                || foundRoleForUser_Alert == null
                || foundRoleForUser_Identity == null)
                CreateRoles();

            /*
             * assign roles to users
             */

            if (!_uow.Users.IsInRole(foundAdmin, foundRoleForAdmin_Alert))
            {
                _uow.Users.AddRole(
                    new uvw_UserRole()
                    {
                        UserId = foundAdmin.Id,
                        RoleId = foundRoleForAdmin_Alert.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }

            if (!_uow.Users.IsInRole(foundUser, foundRoleForUser_Alert))
            {
                _uow.Users.AddRole(
                    new uvw_UserRole()
                    {
                        UserId = foundUser.Id,
                        RoleId = foundRoleForUser_Alert.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }

            if (!_uow.Users.IsInRole(foundAdmin, foundRoleForAdmin_Identity))
            {
                _uow.Users.AddRole(
                    new uvw_UserRole()
                    {
                        UserId = foundAdmin.Id,
                        RoleId = foundRoleForAdmin_Identity.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }

            if (!_uow.Users.IsInRole(foundUser, foundRoleForUser_Identity))
            {
                _uow.Users.AddRole(
                    new uvw_UserRole()
                    {
                        UserId = foundUser.Id,
                        RoleId = foundRoleForUser_Identity.Id,
                        IsDeletable = true,
                    });

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            /*
             * delete default users
             */

            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == DefaultConstants.UserName_Admin || x.UserName == DefaultConstants.UserName_Normal).ToLambda());
            _uow.Commit();

            /*
             * delete default roles
             */

            _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == DefaultConstants.RoleForAdmins_Alert || x.Name == DefaultConstants.RoleForUsers_Alert
                    || x.Name == DefaultConstants.RoleForAdmins_Identity || x.Name == DefaultConstants.RoleForUsers_Identity).ToLambda());
            _uow.Commit();

            /*
             * delete default logins
             */

            _uow.Logins.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == DefaultConstants.LoginName).ToLambda());
            _uow.Commit();

            /*
             * delete default audiences
             */

            _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == DefaultConstants.Audience_Alert || x.Name == DefaultConstants.Audience_Identity).ToLambda());
            _uow.Commit();

            /*
             * delete default issuers
             */

            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == DefaultConstants.IssuerName).ToLambda());
            _uow.Commit();
        }
    }
}
