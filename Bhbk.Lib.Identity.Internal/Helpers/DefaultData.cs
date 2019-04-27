using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
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
        private readonly IIdentityUnitOfWork _uow;

        public DefaultData(IIdentityUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();
        }

        public async Task CreateAsync()
        {
            IssuerCreate issuer;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            //create default issuers
            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                issuer = new IssuerCreate()
                {
                    Name = Constants.ApiDefaultIssuer,
                    IssuerKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = true,
                };

                foundIssuer = await _uow.IssuerRepo.CreateAsync(issuer);

                await _uow.CommitAsync();
            }

            //create default clients
            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Constants.ApiDefaultClientUi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientUi = await _uow.ClientRepo.CreateAsync(client);

                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Constants.ApiDefaultClientApi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = ClientType.server.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientApi = await _uow.ClientRepo.CreateAsync(client);

                await _uow.CommitAsync();
            }

            //create default logins
            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == Constants.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    Name = Constants.ApiDefaultLogin,
                    LoginKey = Constants.ApiUnitTestLoginKey,
                    Enabled = true,
                    Immutable = false,
                };

                foundLogin = await _uow.LoginRepo.CreateAsync(login);

                await _uow.CommitAsync();
            }

            //create default roles
            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientUi.Id,
                    Name = Constants.ApiDefaultRoleForAdmin,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.RoleRepo.CreateAsync(role);
                await _uow.CommitAsync();

                foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == role.Name)).Single();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).SingleOrDefault();

            if (foundRoleForUser == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientUi.Id,
                    Name = Constants.ApiDefaultRoleForUser,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.RoleRepo.CreateAsync(role);
                await _uow.CommitAsync();

                foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == role.Name)).Single();
            }

            //create default users
            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).SingleOrDefault();

            if (foundAdminUser == null)
            {
                user = new UserCreate()
                {
                    Email = Constants.ApiDefaultAdminUser,
                    PhoneNumber = Constants.ApiDefaultAdminUserPhone,
                    FirstName = Constants.ApiDefaultAdminUserFirstName,
                    LastName = Constants.ApiDefaultAdminUserLastName,
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = true,
                };

                await _uow.UserRepo.CreateAsync(user, Constants.ApiDefaultAdminUserPassword);
                await _uow.CommitAsync();

                foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == user.Email)).Single();

                await _uow.UserRepo.SetConfirmedEmailAsync(foundAdminUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundAdminUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundAdminUser.Id, true);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).SingleOrDefault();

            if (foundNormalUser == null)
            {
                user = new UserCreate()
                {
                    Email = Constants.ApiDefaultNormalUser,
                    PhoneNumber = Constants.ApiDefaultNormalUserPhone,
                    FirstName = Constants.ApiDefaultNormalUserFirstName,
                    LastName = Constants.ApiDefaultNormalUserLastName,
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = true,
                };

                await _uow.UserRepo.CreateAsync(user, Constants.ApiDefaultNormalUserPassword);
                await _uow.CommitAsync();

                foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == user.Email)).Single();

                await _uow.UserRepo.SetConfirmedEmailAsync(foundNormalUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundNormalUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundNormalUser.Id, true);
                await _uow.CommitAsync();
            }

            //assign roles, claims & logins to users
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
            //delete default users
            var foundAdminUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser + "@local")).SingleOrDefault();

            if (foundAdminUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundAdminUser.Id);
                await _uow.CommitAsync();
            }

            var foundNormalUser = (await _uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser + "@local")).SingleOrDefault();

            if (foundNormalUser != null)
            {
                await _uow.UserRepo.DeleteAsync(foundNormalUser.Id);
                await _uow.CommitAsync();
            }

            //delete default roles
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

            //delete default logins
            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == Constants.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin != null)
            {
                await _uow.LoginRepo.DeleteAsync(foundLogin.Id);
                await _uow.CommitAsync();
            }

            //delete default clients
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

            //delete default issuers
            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(foundIssuer.Id);
                await _uow.CommitAsync();
            }
        }
    }
}
