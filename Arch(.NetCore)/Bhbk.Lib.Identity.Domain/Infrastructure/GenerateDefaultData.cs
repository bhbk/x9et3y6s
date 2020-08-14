using AutoMapper;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public class GenerateDefaultData
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ValidationHelper _validate;

        public GenerateDefaultData(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
            _validate = new ValidationHelper();
        }

        public void Create()
        {
            /*
             * create default settings
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
                        Immutable = true,
                    }));
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
                        Immutable = true,
                    }));
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
                        Immutable = true,
                    }));
            }

            _uow.Commit();

            /*
             * create default issuers
             */

            var foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == Constants.DefaultIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = Constants.DefaultIssuer,
                        IssuerKey = Constants.DefaultIssuerKey,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default audiences
             */

            var foundAudience_Alert = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.DefaultAudience_Alert).ToLambda())
                .SingleOrDefault();

            if (foundAudience_Alert == null)
            {
                foundAudience_Alert = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.DefaultAudience_Alert,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundAudience_Identity = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.DefaultAudience_Identity).ToLambda())
                .SingleOrDefault();

            if (foundAudience_Identity == null)
            {
                foundAudience_Identity = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.DefaultAudience_Identity,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default logins
             */

            var foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == Constants.DefaultLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Login>(new LoginV1()
                    {
                        Name = Constants.DefaultLogin,
                        LoginKey = Constants.DefaultLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create default roles
             */

            var foundRoleForAdmin_Alert = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForAdmin_Alert).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin_Alert == null)
            {
                foundRoleForAdmin_Alert = _uow.Roles.Create(
                    _mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        Name = Constants.DefaultRoleForAdmin_Alert,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundRoleForUser_Alert = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForUser_Alert).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser_Alert == null)
            {
                foundRoleForUser_Alert = _uow.Roles.Create(
                    _mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Alert.Id,
                        Name = Constants.DefaultRoleForUser_Alert,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundRoleForAdmin_Identity = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForAdmin_Identity).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin_Identity == null)
            {
                foundRoleForAdmin_Identity = _uow.Roles.Create(
                    _mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        Name = Constants.DefaultRoleForAdmin_Identity,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundRoleForUser_Identity = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForUser_Identity).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser_Identity == null)
            {
                foundRoleForUser_Identity = _uow.Roles.Create(
                    _mapper.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience_Identity.Id,
                        Name = Constants.DefaultRoleForUser_Identity,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default users
             */

            var foundAdmin = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.DefaultUser_Admin).ToLambda())
                .SingleOrDefault();

            if (foundAdmin == null)
            {
                foundAdmin = _uow.Users.Create(
                    _mapper.Map<tbl_User>(new UserV1()
                    {
                        UserName = Constants.DefaultUser_Admin,
                        Email = Constants.DefaultUser_Admin,
                        FirstName = Constants.DefaultUserFirstName_Admin,
                        LastName = Constants.DefaultUserLastName_Admin,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), Constants.DefaultUserPass_Admin);

                _uow.Users.SetConfirmedEmail(foundAdmin, true);
                _uow.Users.SetConfirmedPassword(foundAdmin, true);
                _uow.Users.SetConfirmedPhoneNumber(foundAdmin, true);
                _uow.Commit();
            }

            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.DefaultUser_Normal).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<tbl_User>(new UserV1()
                    {
                        UserName = Constants.DefaultUser_Normal,
                        Email = Constants.DefaultUser_Normal,
                        FirstName = Constants.DefaultUserFirstName_Normal,
                        LastName = Constants.DefaultUserLastName_Normal,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), Constants.DefaultUserPass_Normal);

                _uow.Users.SetConfirmedEmail(foundUser, true);
                _uow.Users.SetConfirmedPassword(foundUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundUser, true);

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            _uow.Audiences.SetPasswordHash(foundAudience_Alert, Constants.DefaultAudienceKey_Alert);
            _uow.Audiences.SetPasswordHash(foundAudience_Identity, Constants.DefaultAudienceKey_Identity);

            _uow.Commit();

            /*
             * assign roles to audiences
             */

            if (!_uow.Audiences.IsInRole(foundAudience_Alert, foundRoleForAdmin_Alert))
                _uow.Audiences.AddToRole(foundAudience_Alert, foundRoleForAdmin_Alert);

            if (!_uow.Audiences.IsInRole(foundAudience_Identity, foundRoleForAdmin_Identity))
                _uow.Audiences.AddToRole(foundAudience_Identity, foundRoleForAdmin_Identity);

            _uow.Commit();

            /*
             * assign roles, claims & logins to users
             */

            if (!_uow.Users.IsInLogin(foundAdmin, foundLogin))
                _uow.Users.AddToLogin(foundAdmin, foundLogin);

            if (!_uow.Users.IsInLogin(foundUser, foundLogin))
                _uow.Users.AddToLogin(foundUser, foundLogin);

            if (!_uow.Users.IsInRole(foundAdmin, foundRoleForAdmin_Alert))
                _uow.Users.AddToRole(foundAdmin, foundRoleForAdmin_Alert);

            if (!_uow.Users.IsInRole(foundUser, foundRoleForUser_Alert))
                _uow.Users.AddToRole(foundUser, foundRoleForUser_Alert);

            if (!_uow.Users.IsInRole(foundAdmin, foundRoleForAdmin_Identity))
                _uow.Users.AddToRole(foundAdmin, foundRoleForAdmin_Identity);

            if (!_uow.Users.IsInRole(foundUser, foundRoleForUser_Identity))
                _uow.Users.AddToRole(foundUser, foundRoleForUser_Identity);

            _uow.Commit();
        }

        public void Destroy()
        {
            /*
             * delete default users
             */

            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.DefaultUser_Admin || x.UserName == Constants.DefaultUser_Normal).ToLambda());

            _uow.Commit();

            /*
             * delete default roles
             */

            _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == Constants.DefaultRoleForAdmin_Alert || x.Name == Constants.DefaultRoleForUser_Alert
                    || x.Name == Constants.DefaultRoleForAdmin_Identity || x.Name == Constants.DefaultRoleForUser_Identity).ToLambda());

            _uow.Commit();

            /*
             * delete default logins
             */

            _uow.Logins.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == Constants.DefaultLogin).ToLambda());

            _uow.Commit();

            /*
             * delete default audiences
             */

            _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.DefaultAudience_Alert || x.Name == Constants.DefaultAudience_Identity).ToLambda());

            _uow.Commit();

            /*
             * delete default issuers
             */

            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == Constants.DefaultIssuer).ToLambda());

            _uow.Commit();
        }
    }
}
