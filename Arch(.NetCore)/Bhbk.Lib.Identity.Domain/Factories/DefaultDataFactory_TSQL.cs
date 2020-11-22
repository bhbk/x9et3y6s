using AutoMapper;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TSQL;
using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public class DefaultDataFactory_TSQL : IDefaultDataFactory_TSQL
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private uvw_Issuer foundIssuer;
        private uvw_Audience foundAudience_Alert, foundAudience_Identity;
        private uvw_Login foundLogin;
        private uvw_Role foundRoleForAdmin_Alert, foundRoleForAdmin_Identity, foundRoleForUser_Alert, foundRoleForUser_Identity;
        private uvw_User foundAdmin, foundUser;
        private uvw_Setting foundGlobalLegacyClaims, foundGlobalLegacyIssuer, foundGlobalTotpExpire;

        public DefaultDataFactory_TSQL(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TSQL>()).CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create default audiences
             */

            foundAudience_Alert = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.DefaultAudience_Alert).ToLambda())
                .SingleOrDefault();

            if (foundAudience_Alert == null)
            {
                foundAudience_Alert = _uow.Audiences.Create(
                    _mapper.Map<uvw_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.DefaultAudience_Alert,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundAudience_Identity = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.DefaultAudience_Identity).ToLambda())
                .SingleOrDefault();

            if (foundAudience_Identity == null)
            {
                foundAudience_Identity = _uow.Audiences.Create(
                    _mapper.Map<uvw_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.DefaultAudience_Identity,
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
                _uow.Audiences.SetPassword(foundAudience_Alert, Constants.DefaultAudiencePassword_Alert);
                _uow.Commit();
            }

            if (!_uow.Audiences.IsPasswordSet(foundAudience_Identity))
            {
                _uow.Audiences.SetPassword(foundAudience_Identity, Constants.DefaultAudiencePassword_Identity);
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
                .Where(x => x.Name == Constants.DefaultIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<uvw_Issuer>(new IssuerV1()
                    {
                        Name = Constants.DefaultIssuer,
                        IssuerKey = Constants.DefaultIssuerKey,
                        IsEnabled = true,
                        IsDeletable = false,
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
             * create default logins
             */

            foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == Constants.DefaultLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<uvw_Login>(new LoginV1()
                    {
                        Name = Constants.DefaultLogin,
                        LoginKey = Constants.DefaultLoginKey,
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
                .Where(x => x.Name == Constants.DefaultRoleForAdmin_Alert).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin_Alert == null)
            {
                foundRoleForAdmin_Alert = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        Name = Constants.DefaultRoleForAdmin_Alert,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundRoleForUser_Alert = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForUser_Alert).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser_Alert == null)
            {
                foundRoleForUser_Alert = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        Name = Constants.DefaultRoleForUser_Alert,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundRoleForAdmin_Identity = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForAdmin_Identity).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin_Identity == null)
            {
                foundRoleForAdmin_Identity = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        Name = Constants.DefaultRoleForAdmin_Identity,
                        IsEnabled = true,
                        IsDeletable = false,
                    }));

                _uow.Commit();
            }

            foundRoleForUser_Identity = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForUser_Identity).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser_Identity == null)
            {
                foundRoleForUser_Identity = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        Name = Constants.DefaultRoleForUser_Identity,
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
                && x.ConfigKey == Constants.SettingGlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<uvw_Setting>(new SettingV1()
                    {
                        ConfigKey = Constants.SettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        IsDeletable = false,
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
                        IsDeletable = false,
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
                .Where(x => x.UserName == Constants.DefaultUser_Admin).ToLambda())
                .SingleOrDefault();

            if (foundAdmin == null)
            {
                foundAdmin = _uow.Users.Create(
                    _mapper.Map<uvw_User>(new UserV1()
                    {
                        UserName = Constants.DefaultUser_Admin,
                        Email = Constants.DefaultUser_Admin,
                        FirstName = Constants.DefaultUserFirstName_Admin,
                        LastName = Constants.DefaultUserLastName_Admin,
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }), Constants.DefaultUserPass_Admin);

                _uow.Users.SetConfirmedEmail(foundAdmin, true);
                _uow.Users.SetConfirmedPhoneNumber(foundAdmin, true);
                _uow.Users.SetConfirmedPassword(foundAdmin, true);

                _uow.Commit();
            }

            foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.DefaultUser_Normal).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<uvw_User>(new UserV1()
                    {
                        UserName = Constants.DefaultUser_Normal,
                        Email = Constants.DefaultUser_Normal,
                        FirstName = Constants.DefaultUserFirstName_Normal,
                        LastName = Constants.DefaultUserLastName_Normal,
                        IsHumanBeing = true,
                        IsLockedOut = false,
                        IsDeletable = false,
                    }), Constants.DefaultUserPass_Normal);

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
                .Where(x => x.UserName == Constants.DefaultUser_Admin || x.UserName == Constants.DefaultUser_Normal).ToLambda());
            _uow.Commit();

            /*
             * delete default roles
             */

            _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForAdmin_Alert || x.Name == Constants.DefaultRoleForUser_Alert
                    || x.Name == Constants.DefaultRoleForAdmin_Identity || x.Name == Constants.DefaultRoleForUser_Identity).ToLambda());
            _uow.Commit();

            /*
             * delete default logins
             */

            _uow.Logins.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == Constants.DefaultLogin).ToLambda());
            _uow.Commit();

            /*
             * delete default audiences
             */

            _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.DefaultAudience_Alert || x.Name == Constants.DefaultAudience_Identity).ToLambda());
            _uow.Commit();

            /*
             * delete default issuers
             */

            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.DefaultIssuer).ToLambda());
            _uow.Commit();
        }
    }
}
