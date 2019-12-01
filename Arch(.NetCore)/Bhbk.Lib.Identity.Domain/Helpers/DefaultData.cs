using AutoMapper;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public class DefaultData
    {
        private readonly IUoWService _uow;
        private readonly IMapper _mapper;

        public DefaultData(IUoWService uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public void Create()
        {
            /*
             * create default settings
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
             * create default msg of the day
             */

            _uow.MOTDs.Create(
                new tbl_MotDType1()
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = DateTime.Now,
                    Author = "Albert Einstein",
                    Quote = "Logic will get you from A to B. Imagination will take you everywhere.",
                    Length = 69,
                    Category = "inspire",
                    Title = "Inspiring Quote of the day",
                    Background = "https://theysaidso.com/img/bgs/man_on_the_mountain.jpg",
                    Tags = "imagination,inspire,t-shirt,tod",
                });

            _uow.MOTDs.Create(
                new tbl_MotDType1()
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = DateTime.Now,
                    Author = "Vincent Van Gogh",
                    Quote = "Great things are done by a series of small things brought together.",
                    Length = 67,
                    Category = "inspire",
                    Title = "Inspiring Quote of the day",
                    Background = "https://theysaidso.com/img/bgs/man_on_the_mountain.jpg",
                    Tags = "inspire,small-things,tod,tso-art",
                });

            _uow.Commit();

            /*
             * create default issuers
             */

            var foundIssuer = _uow.Issuers.Get(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name == RealConstants.ApiDefaultIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = RealConstants.ApiDefaultIssuer,
                        IssuerKey = RealConstants.ApiDefaultIssuerKey,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default audiences
             */

            var foundClientUi = _uow.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == RealConstants.ApiDefaultAudienceUi).ToLambda())
                .SingleOrDefault();

            if (foundClientUi == null)
            {
                foundClientUi = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = RealConstants.ApiDefaultAudienceUi,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundClientApi = _uow.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == RealConstants.ApiDefaultAudienceApi).ToLambda())
                .SingleOrDefault();

            if (foundClientApi == null)
            {
                foundClientApi = _uow.Audiences.Create(
                     _mapper.Map<tbl_Audiences>(new AudienceCreate()
                     {
                         IssuerId = foundIssuer.Id,
                         Name = RealConstants.ApiDefaultAudienceApi,
                         AudienceType = AudienceType.server.ToString(),
                         Enabled = true,
                         Immutable = true,
                     }));

                _uow.Commit();
            }

            /*
             * create default logins
             */

            var foundLogin = _uow.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == RealConstants.ApiDefaultLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = RealConstants.ApiDefaultLogin,
                        LoginKey = RealConstants.ApiDefaultLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create default roles
             */

            var foundRoleForAdmin = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == RealConstants.ApiDefaultRoleForAdmin).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                foundRoleForAdmin = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = foundClientUi.Id,
                        Name = RealConstants.ApiDefaultRoleForAdmin,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundRoleForUser = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == RealConstants.ApiDefaultRoleForUser).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser == null)
            {
                foundRoleForUser = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = foundClientUi.Id,
                        Name = RealConstants.ApiDefaultRoleForUser,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default users
             */

            var foundAdminUser = _uow.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == RealConstants.ApiDefaultAdminUser).ToLambda())
                .SingleOrDefault();

            if (foundAdminUser == null)
            {
                foundAdminUser = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = RealConstants.ApiDefaultAdminUser,
                        PhoneNumber = RealConstants.ApiDefaultAdminUserPhone,
                        FirstName = RealConstants.ApiDefaultAdminUserFirstName,
                        LastName = RealConstants.ApiDefaultAdminUserLastName,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), RealConstants.ApiDefaultAdminUserPassword);

                _uow.Users.SetConfirmedEmail(foundAdminUser, true);
                _uow.Users.SetConfirmedPassword(foundAdminUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundAdminUser, true);
                _uow.Commit();
            }

            var foundNormalUser = _uow.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == RealConstants.ApiDefaultNormalUser).ToLambda())
                .SingleOrDefault();

            if (foundNormalUser == null)
            {
                foundNormalUser = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = RealConstants.ApiDefaultNormalUser,
                        PhoneNumber = RealConstants.ApiDefaultNormalUserPhone,
                        FirstName = RealConstants.ApiDefaultNormalUserFirstName,
                        LastName = RealConstants.ApiDefaultNormalUserLastName,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), RealConstants.ApiDefaultNormalUserPassword);

                _uow.Users.SetConfirmedEmail(foundNormalUser, true);
                _uow.Users.SetConfirmedPassword(foundNormalUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundNormalUser, true);

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            _uow.Audiences.SetPassword(foundClientApi, RealConstants.ApiDefaultAudienceApiKey);
            _uow.Audiences.SetPassword(foundClientUi, RealConstants.ApiDefaultAudienceUiKey);

            _uow.Commit();

            /*
             * assign roles, claims & logins to users
             */

            if (!_uow.Users.IsInLogin(foundAdminUser.Id, foundLogin.Id))
                _uow.Users.AddToLogin(foundAdminUser, foundLogin);

            if (!_uow.Users.IsInRole(foundAdminUser.Id, foundRoleForAdmin.Id))
                _uow.Users.AddToRole(foundAdminUser, foundRoleForAdmin);

            if (!_uow.Users.IsInLogin(foundNormalUser.Id, foundLogin.Id))
                _uow.Users.AddToLogin(foundNormalUser, foundLogin);

            if (!_uow.Users.IsInRole(foundNormalUser.Id, foundRoleForUser.Id))
                _uow.Users.AddToRole(foundNormalUser, foundRoleForUser);

            _uow.Commit();
        }

        public void Destroy()
        {
            /*
             * delete default users
             */

            _uow.Users.Delete(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == RealConstants.ApiDefaultAdminUser).ToLambda());

            _uow.Users.Delete(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == RealConstants.ApiDefaultNormalUser).ToLambda());

            _uow.Commit();

            /*
             * delete default roles
             */

            _uow.Roles.Delete(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == RealConstants.ApiDefaultRoleForAdmin).ToLambda());

            _uow.Roles.Delete(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == RealConstants.ApiDefaultRoleForUser).ToLambda());

            _uow.Commit();

            /*
             * delete default logins
             */

            _uow.Logins.Delete(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == RealConstants.ApiDefaultLogin).ToLambda());

            _uow.Commit();

            /*
             * delete default audiences
             */

            _uow.Audiences.Delete(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == RealConstants.ApiDefaultAudienceUi).ToLambda());

            _uow.Audiences.Delete(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == RealConstants.ApiDefaultAudienceApi).ToLambda());

            _uow.Commit();

            /*
             * delete default issuers
             */

            _uow.Issuers.Delete(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name == RealConstants.ApiDefaultIssuer).ToLambda());

            _uow.Commit();
        }
    }
}
