using AutoMapper;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Data.EFCore.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public class GenerateDefaultData
    {
        private readonly IUoWService _uow;
        private readonly IMapper _mapper;
        private readonly ValidationHelper _validate;

        public GenerateDefaultData(IUoWService uow, IMapper mapper)
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
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyClaims).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = _uow.Settings.Create(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
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
                    _mapper.Map<tbl_Settings>(new SettingCreate()
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
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiSettingGlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        Immutable = true,
                    }));
            }

            /*
             * create default msg of the day
             */

            _uow.MOTDs.Create(
                new tbl_MOTDs()
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
                new tbl_MOTDs()
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
                .Where(x => x.Name == Constants.ApiDefaultIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiDefaultIssuer,
                        IssuerKey = Constants.ApiDefaultIssuerKey,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default audiences
             */

            var foundAudienceUi = _uow.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiDefaultAudienceUi).ToLambda())
                .SingleOrDefault();

            if (foundAudienceUi == null)
            {
                foundAudienceUi = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiDefaultAudienceUi,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundAudienceApi = _uow.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiDefaultAudienceApi).ToLambda())
                .SingleOrDefault();

            if (foundAudienceApi == null)
            {
                foundAudienceApi = _uow.Audiences.Create(
                     _mapper.Map<tbl_Audiences>(new AudienceCreate()
                     {
                         IssuerId = foundIssuer.Id,
                         Name = Constants.ApiDefaultAudienceApi,
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
                .Where(x => x.Name == Constants.ApiDefaultLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = Constants.ApiDefaultLogin,
                        LoginKey = Constants.ApiDefaultLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create default roles
             */

            var foundRoleForAdmin = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiDefaultRoleForAdmin).ToLambda())
                .SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                foundRoleForAdmin = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = foundAudienceUi.Id,
                        Name = Constants.ApiDefaultRoleForAdmin,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundRoleForUser = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiDefaultRoleForUser).ToLambda())
                .SingleOrDefault();

            if (foundRoleForUser == null)
            {
                foundRoleForUser = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = foundAudienceUi.Id,
                        Name = Constants.ApiDefaultRoleForUser,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            var foundRoleForService = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiDefaultRoleForService).ToLambda())
                .SingleOrDefault();

            if (foundRoleForService == null)
            {
                foundRoleForService = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = foundAudienceApi.Id,
                        Name = Constants.ApiDefaultRoleForService,
                        Enabled = true,
                        Immutable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create default users
             */

            var foundAdminUser = _uow.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == Constants.ApiDefaultAdminUser).ToLambda())
                .SingleOrDefault();

            if (foundAdminUser == null)
            {
                foundAdminUser = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = Constants.ApiDefaultAdminUser,
                        PhoneNumber = Constants.ApiDefaultAdminUserPhone,
                        FirstName = Constants.ApiDefaultAdminUserFirstName,
                        LastName = Constants.ApiDefaultAdminUserLastName,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), new ValidationHelper().PasswordHash(Constants.ApiDefaultAdminUserPassword));

                _uow.Users.SetConfirmedEmail(foundAdminUser, true);
                _uow.Users.SetConfirmedPassword(foundAdminUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundAdminUser, true);
                _uow.Commit();
            }

            var foundNormalUser = _uow.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == Constants.ApiDefaultNormalUser).ToLambda())
                .SingleOrDefault();

            if (foundNormalUser == null)
            {
                foundNormalUser = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = Constants.ApiDefaultNormalUser,
                        PhoneNumber = Constants.ApiDefaultNormalUserPhone,
                        FirstName = Constants.ApiDefaultNormalUserFirstName,
                        LastName = Constants.ApiDefaultNormalUserLastName,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), new ValidationHelper().PasswordHash(Constants.ApiDefaultNormalUserPassword));

                _uow.Users.SetConfirmedEmail(foundNormalUser, true);
                _uow.Users.SetConfirmedPassword(foundNormalUser, true);
                _uow.Users.SetConfirmedPhoneNumber(foundNormalUser, true);

                _uow.Commit();
            }

            /*
             * set password to audiences
             */

            _uow.Audiences.SetPasswordHash(foundAudienceApi, new ValidationHelper().PasswordHash(Constants.ApiDefaultAudienceApiKey));
            _uow.Audiences.SetPasswordHash(foundAudienceUi, new ValidationHelper().PasswordHash(Constants.ApiDefaultAudienceUiKey));

            _uow.Commit();

            /*
             * assign roles to audiences
             */

            if (!_uow.Audiences.IsInRole(foundAudienceApi, foundRoleForService))
                _uow.Audiences.AddToRole(foundAudienceApi, foundRoleForService);

            _uow.Commit();

            /*
             * assign roles, claims & logins to users
             */

            if (!_uow.Users.IsInLogin(foundAdminUser, foundLogin))
                _uow.Users.AddToLogin(foundAdminUser, foundLogin);

            if (!_uow.Users.IsInLogin(foundNormalUser, foundLogin))
                _uow.Users.AddToLogin(foundNormalUser, foundLogin);

            if (!_uow.Users.IsInRole(foundAdminUser, foundRoleForAdmin))
                _uow.Users.AddToRole(foundAdminUser, foundRoleForAdmin);

            if (!_uow.Users.IsInRole(foundNormalUser, foundRoleForUser))
                _uow.Users.AddToRole(foundNormalUser, foundRoleForUser);

            _uow.Commit();
        }

        public void Destroy()
        {
            /*
             * delete default users
             */

            _uow.Users.Delete(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == Constants.ApiDefaultAdminUser).ToLambda());

            _uow.Users.Delete(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == Constants.ApiDefaultNormalUser).ToLambda());

            _uow.Commit();

            /*
             * delete default roles
             */

            _uow.Roles.Delete(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiDefaultRoleForAdmin).ToLambda());

            _uow.Roles.Delete(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiDefaultRoleForUser).ToLambda());

            _uow.Commit();

            /*
             * delete default logins
             */

            _uow.Logins.Delete(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == Constants.ApiDefaultLogin).ToLambda());

            _uow.Commit();

            /*
             * delete default audiences
             */

            _uow.Audiences.Delete(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiDefaultAudienceUi).ToLambda());

            _uow.Audiences.Delete(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiDefaultAudienceApi).ToLambda());

            _uow.Commit();

            /*
             * delete default issuers
             */

            _uow.Issuers.Delete(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name == Constants.ApiDefaultIssuer).ToLambda());

            _uow.Commit();
        }
    }
}
