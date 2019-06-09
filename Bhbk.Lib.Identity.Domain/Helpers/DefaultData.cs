using AutoMapper;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task CreateAsync()
        {
            /*
             * create default settings
             */

            var foundGlobalLegacyClaims = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyClaims)).SingleOrDefault();

            if (foundGlobalLegacyClaims == null)
            {
                foundGlobalLegacyClaims = await _uow.SettingRepo.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyClaims,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalLegacyIssuer = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalLegacyIssuer)).SingleOrDefault();

            if (foundGlobalLegacyIssuer == null)
            {
                foundGlobalLegacyIssuer = await _uow.SettingRepo.CreateAsync(
                    _mapper.Map<tbl_Settings>(new SettingCreate()
                    {
                        ConfigKey = RealConstants.ApiSettingGlobalLegacyIssuer,
                        ConfigValue = "true",
                        Immutable = true,
                    }));
            }

            var foundGlobalTotpExpire = (await _uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingGlobalTotpExpire)).SingleOrDefault();

            if (foundGlobalTotpExpire == null)
            {
                foundGlobalTotpExpire = await _uow.SettingRepo.CreateAsync(
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

            await _uow.UserRepo.CreateMOTDAsync(
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

            await _uow.UserRepo.CreateMOTDAsync(
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

            await _uow.CommitAsync();

            /*
             * create default issuers
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = await _uow.IssuerRepo.CreateAsync(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = RealConstants.ApiDefaultIssuer,
                        IssuerKey = RealConstants.ApiDefaultIssuerKey,
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create default clients
             */

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi == null)
            {
                foundClientUi = await _uow.ClientRepo.CreateAsync(
                    _mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = RealConstants.ApiDefaultClientUi,
                        ClientKey = RealConstants.ApiDefaultClientUiKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi == null)
            {
                foundClientApi = await _uow.ClientRepo.CreateAsync(
                     _mapper.Map<tbl_Clients>(new ClientCreate()
                     {
                         IssuerId = foundIssuer.Id,
                         Name = RealConstants.ApiDefaultClientApi,
                         ClientKey = RealConstants.ApiDefaultClientApiKey,
                         ClientType = ClientType.server.ToString(),
                         Enabled = true,
                         Immutable = true,
                     }));

                await _uow.CommitAsync();
            }

            /*
             * create default logins
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = await _uow.LoginRepo.CreateAsync(
                    _mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = RealConstants.ApiDefaultLogin,
                        LoginKey = RealConstants.ApiDefaultLoginKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create default roles
             */

            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                foundRoleForAdmin = await _uow.RoleRepo.CreateAsync(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = foundClientUi.Id,
                        Name = RealConstants.ApiDefaultRoleForAdmin,
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultRoleForUser)).SingleOrDefault();

            if (foundRoleForUser == null)
            {
                foundRoleForUser = await _uow.RoleRepo.CreateAsync(
                    _mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = foundClientUi.Id,
                        Name = RealConstants.ApiDefaultRoleForUser,
                        Enabled = true,
                        Immutable = true,
                    }));

                await _uow.CommitAsync();
            }

            /*
             * create default users
             */

            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).SingleOrDefault();

            if (foundAdminUser == null)
            {
                foundAdminUser = await _uow.UserRepo.CreateAsync(
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

                await _uow.UserRepo.SetConfirmedEmailAsync(foundAdminUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundAdminUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundAdminUser.Id, true);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).SingleOrDefault();

            if (foundNormalUser == null)
            {
                foundNormalUser = await _uow.UserRepo.CreateAsync(
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

            await _uow.CommitAsync();
        }

        public async Task DestroyAsync()
        {
            /*
             * delete default users
             */

            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).SingleOrDefault();

            if (foundAdminUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundAdminUser.Id);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).SingleOrDefault();

            if (foundNormalUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundNormalUser.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default roles
             */

            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (foundRoleForAdmin != null)
            {
                await _uow.RoleRepo.DeleteAsync(foundRoleForAdmin.Id);
                await _uow.CommitAsync();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultRoleForUser)).SingleOrDefault();

            if (foundRoleForUser != null)
            {
                await _uow.RoleRepo.DeleteAsync(foundRoleForUser.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default logins
             */

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin != null)
            {
                await _uow.LoginRepo.DeleteAsync(foundLogin.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default clients
             */

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi != null)
            {
                await _uow.ClientRepo.DeleteAsync(foundClientUi.Id);
                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi != null)
            {
                await _uow.ClientRepo.DeleteAsync(foundClientApi.Id);
                await _uow.CommitAsync();
            }

            /*
             * delete default issuers
             */

            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(foundIssuer.Id);
                await _uow.CommitAsync();
            }
        }
    }
}
