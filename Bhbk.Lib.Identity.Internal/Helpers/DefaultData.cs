using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Helpers
{
    public class DefaultData
    {
        private readonly IUnitOfWork _uow;

        public DefaultData(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();
        }

        public async Task CreateAsync()
        {
            /*
             * create settings
             */

            var foundGlobalLegacyClaims = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyClaims)).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiSettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalLegacyIssuer = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalLegacyIssuer)).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiSettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalTotpExpire = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingGlobalTotpExpire)).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiSettingGlobalTotpExpire,
                        ConfigValue = 1200.ToString(),
                        Immutable = true,
                    }));
            }

            /*
             * create default issuer
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = await _uow.IssuerRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiDefaultIssuer,
                        IssuerKey = Constants.ApiDefaultIssuerKey,
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create default clients
             */

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi == null)
            {
                foundClientUi = await _uow.ClientRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiDefaultClientUi,
                        ClientKey = Constants.ApiDefaultClientUiKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi == null)
            {
                foundClientApi = await _uow.ClientRepo.CreateAsync(
                     _uow.Mapper.Map<tbl_Clients>(new ClientCreate()
                     {
                         IssuerId = foundIssuer.Id,
                         Name = Constants.ApiDefaultClientApi,
                         ClientKey = Constants.ApiDefaultClientApiKey,
                         ClientType = ClientType.server.ToString(),
                         Enabled = true,
                         Immutable = true,
                     }));

                await _uow.CommitAsync();
            }

            /*
             * create default login
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == Constants.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = await _uow.LoginRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = Constants.ApiDefaultLogin,
                        LoginKey = Constants.ApiDefaultLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create default roles
             */

            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                foundRoleForAdmin = await _uow.RoleRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = foundClientUi.Id,
                        Name = Constants.ApiDefaultRoleForAdmin,
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).SingleOrDefault();

            if (foundRoleForUser == null)
            {
                foundRoleForUser = await _uow.RoleRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = foundClientUi.Id,
                        Name = Constants.ApiDefaultRoleForUser,
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create default users
             */

            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).SingleOrDefault();

            if (foundAdminUser == null)
            {
                foundAdminUser = await _uow.UserRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = Constants.ApiDefaultAdminUser,
                        PhoneNumber = Constants.ApiDefaultAdminUserPhone,
                        FirstName = Constants.ApiDefaultAdminUserFirstName,
                        LastName = Constants.ApiDefaultAdminUserLastName,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), Constants.ApiDefaultAdminUserPassword);

                await _uow.UserRepo.SetConfirmedEmailAsync(foundAdminUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundAdminUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundAdminUser.Id, true);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).SingleOrDefault();

            if (foundNormalUser == null)
            {
                foundNormalUser = await _uow.UserRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = Constants.ApiDefaultNormalUser,
                        PhoneNumber = Constants.ApiDefaultNormalUserPhone,
                        FirstName = Constants.ApiDefaultNormalUserFirstName,
                        LastName = Constants.ApiDefaultNormalUserLastName,
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = true,
                    }), Constants.ApiDefaultNormalUserPassword);

                await _uow.UserRepo.SetConfirmedEmailAsync(foundNormalUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundNormalUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundNormalUser.Id, true);
                await _uow.CommitAsync();
            }

            /*
             * assign roles, claims & logins to users
             */

            if (!await _uow.UserRepo.IsInLoginAsync(foundAdminUser.Id, foundLogin.Id))
                await _uow.UserRepo.AddToLoginAsync(foundAdminUser, foundLogin);

            if (!await _uow.UserRepo.IsInRoleAsync(foundAdminUser.Id, foundRoleForAdmin.Id))
                await _uow.UserRepo.AddToRoleAsync(foundAdminUser, foundRoleForAdmin);

            if (!await _uow.UserRepo.IsInLoginAsync(foundNormalUser.Id, foundLogin.Id))
                await _uow.UserRepo.AddToLoginAsync(foundNormalUser, foundLogin);

            if (!await _uow.UserRepo.IsInRoleAsync(foundNormalUser.Id, foundRoleForUser.Id))
                await _uow.UserRepo.AddToRoleAsync(foundNormalUser, foundRoleForUser);
        }

        public async Task DestroyAsync()
        {
            /*
             * delete default users
             */

            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).SingleOrDefault();

            if (foundAdminUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundAdminUser.Id);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).SingleOrDefault();

            if (foundNormalUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundNormalUser.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default roles
             */

            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (foundRoleForAdmin != null)
            {
                await _uow.RoleRepo.DeleteAsync(foundRoleForAdmin.Id);
                await _uow.CommitAsync();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).SingleOrDefault();

            if (foundRoleForUser != null)
            {
                await _uow.RoleRepo.DeleteAsync(foundRoleForUser.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default logins
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == Constants.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin != null)
            {
                await _uow.LoginRepo.DeleteAsync(foundLogin.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default clients
             */

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi != null)
            {
                await _uow.ClientRepo.DeleteAsync(foundClientUi.Id);
                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi != null)
            {
                await _uow.ClientRepo.DeleteAsync(foundClientApi.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default issuers
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(foundIssuer.Id);
                await _uow.CommitAsync();
            }
        }
    }
}
