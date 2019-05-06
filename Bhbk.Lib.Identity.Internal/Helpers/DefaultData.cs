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
             * create default issuer
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultIssuer))).FirstOrDefault();

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

            var foundExpireAccess = (await _uow.SettingRepo.GetAsync(x => x.ConfigKey.Contains(Constants.ApiDefaultSettingExpireAccess))).FirstOrDefault();

            if(foundExpireAccess == null)
            {
                foundExpireAccess = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.ApiDefaultSettingExpireAccess,
                        ConfigValue = 600.ToString(),
                        Immutable = true,
                    }));
            }

            var foundExpireRefresh = (await _uow.SettingRepo.GetAsync(x => x.ConfigKey.Contains(Constants.ApiDefaultSettingExpireRefresh))).FirstOrDefault();

            if (foundExpireRefresh == null)
            {
                foundExpireRefresh = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        ConfigKey = Constants.ApiDefaultSettingExpireRefresh,
                        ConfigValue = 86400.ToString(),
                        Immutable = true,
                    }));
            }

            var foundExpireTotp = (await _uow.SettingRepo.GetAsync(x => x.ConfigKey.Contains(Constants.ApiDefaultSettingExpireTotp))).FirstOrDefault();

            if (foundExpireTotp == null)
            {
                foundExpireTotp = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiDefaultSettingExpireTotp,
                        ConfigValue = 600.ToString(),
                        Immutable = true,
                    }));
            }

            var foundLegacyIssuer = (await _uow.SettingRepo.GetAsync(x => x.ConfigKey.Contains(Constants.ApiDefaultSettingLegacyIssuer))).FirstOrDefault();

            if (foundLegacyIssuer == null)
            {
                foundLegacyIssuer = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiDefaultSettingLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundLegacyClaims = (await _uow.SettingRepo.GetAsync(x => x.ConfigKey.Contains(Constants.ApiDefaultSettingLegacyClaims))).FirstOrDefault();

            if (foundLegacyClaims == null)
            {
                foundLegacyClaims = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiDefaultSettingLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundPollingMax = (await _uow.SettingRepo.GetAsync(x => x.ConfigKey.Contains(Constants.ApiDefaultSettingPollingMax))).FirstOrDefault();

            if (foundPollingMax == null)
            {
                foundPollingMax = await _uow.SettingRepo.CreateAsync(
                    _uow.Mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = Constants.ApiDefaultSettingPollingMax,
                        ConfigValue = 10.ToString(),
                        Immutable = true,
                    }));
            }

            await _uow.CommitAsync();

            /*
             * create default clients
             */

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultClientUi))).FirstOrDefault();

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

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultClientApi))).FirstOrDefault();

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

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultLogin))).FirstOrDefault();

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

            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultRoleForAdmin))).FirstOrDefault();

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

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultRoleForUser))).FirstOrDefault();

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

            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email.Contains(Constants.ApiDefaultAdminUser))).FirstOrDefault();

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

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email.Contains(Constants.ApiDefaultNormalUser))).FirstOrDefault();

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
                await _uow.UserRepo.AddLoginAsync(foundAdminUser, foundLogin);

            if (!await _uow.UserRepo.IsInRoleAsync(foundAdminUser.Id, foundRoleForAdmin.Id))
                await _uow.UserRepo.AddRoleAsync(foundAdminUser, foundRoleForAdmin);

            if (!await _uow.UserRepo.IsInLoginAsync(foundNormalUser.Id, foundLogin.Id))
                await _uow.UserRepo.AddLoginAsync(foundNormalUser, foundLogin);

            if (!await _uow.UserRepo.IsInRoleAsync(foundNormalUser.Id, foundRoleForUser.Id))
                await _uow.UserRepo.AddRoleAsync(foundNormalUser, foundRoleForUser);
        }

        public async Task DestroyAsync()
        {
            /*
             * delete default users
             */

            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email.Contains(Constants.ApiDefaultAdminUser))).SingleOrDefault();

            if (foundAdminUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundAdminUser.Id);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email.Contains(Constants.ApiDefaultNormalUser))).SingleOrDefault();

            if (foundNormalUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundNormalUser.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default roles
             */

            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultRoleForAdmin))).SingleOrDefault();

            if (foundRoleForAdmin != null)
            {
                await _uow.RoleRepo.DeleteAsync(foundRoleForAdmin.Id);
                await _uow.CommitAsync();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultRoleForUser))).SingleOrDefault();

            if (foundRoleForUser != null)
            {
                await _uow.RoleRepo.DeleteAsync(foundRoleForUser.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default logins
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultLogin))).SingleOrDefault();

            if (foundLogin != null)
            {
                await _uow.LoginRepo.DeleteAsync(foundLogin.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default clients
             */

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultClientUi))).SingleOrDefault();

            if (foundClientUi != null)
            {
                await _uow.ClientRepo.DeleteAsync(foundClientUi.Id);
                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultClientApi))).SingleOrDefault();

            if (foundClientApi != null)
            {
                await _uow.ClientRepo.DeleteAsync(foundClientApi.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default issuers
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Constants.ApiDefaultIssuer))).SingleOrDefault();

            if (foundIssuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(foundIssuer.Id);
                await _uow.CommitAsync();
            }
        }
    }
}
