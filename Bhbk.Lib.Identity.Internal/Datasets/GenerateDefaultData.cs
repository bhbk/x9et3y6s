using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Datasets
{
    public class GenerateDefaultData
    {
        private readonly IIdentityContext<DatabaseContext> _uow;

        public GenerateDefaultData(IIdentityContext<DatabaseContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async Task CreateAsync()
        {
            IssuerCreate issuer;
            ClientCreate client;
            LoginCreate login;
            RoleCreate role;
            UserCreate user;

            //create default issuers
            var foundIssuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).SingleOrDefault();

            if (foundIssuer == null)
            {
                issuer = new IssuerCreate()
                {
                    Name = Strings.ApiDefaultIssuer,
                    IssuerKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = true,
                };

                foundIssuer = await _uow.IssuerRepo.CreateAsync(issuer);

                await _uow.CommitAsync();
            }

            //create default clients
            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Strings.ApiDefaultClientApi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = ClientType.server.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientApi = await _uow.ClientRepo.CreateAsync(client);

                await _uow.CommitAsync();
            }

            var foundClientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).SingleOrDefault();

            if (foundClientUi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Strings.ApiDefaultClientUi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientUi = await _uow.ClientRepo.CreateAsync(client);

                await _uow.CommitAsync();
            }

            //create default logins
            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.Name == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    Name = Strings.ApiDefaultLogin,
                    LoginKey = Strings.ApiUnitTestLogin1Key,
                    Enabled = true,
                    Immutable = false,
                };

                foundLogin = await _uow.LoginRepo.CreateAsync(login);

                await _uow.CommitAsync();
            }

            //create default roles
            var foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (foundRoleForAdmin == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientUi.Id,
                    Name = Strings.ApiDefaultRoleForAdmin,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.RoleRepo.CreateAsync(role);
                await _uow.CommitAsync();

                foundRoleForAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == role.Name)).Single();
            }

            var foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForUser)).SingleOrDefault();

            if (foundRoleForUser == null)
            {
                role = new RoleCreate()
                {
                    ClientId = foundClientApi.Id,
                    Name = Strings.ApiDefaultRoleForUser,
                    Enabled = true,
                    Immutable = true,
                };

                await _uow.RoleRepo.CreateAsync(role);
                await _uow.CommitAsync();

                foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == role.Name)).Single();
            }

            //create default users
            var foundUser = (await _uow.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).SingleOrDefault();

            if (foundUser == null)
            {
                user = new UserCreate()
                {
                    Email = Strings.ApiDefaultUserAdmin,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "Identity",
                    LastName = "System",
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = true,
                };

                await _uow.UserRepo.CreateAsync(user, Strings.ApiDefaultUserPassword);
                await _uow.CommitAsync();

                foundUser = (await _uow.UserRepo.GetAsync(x => x.Email == user.Email)).Single();

                await _uow.UserRepo.SetConfirmedEmailAsync(foundUser.Id, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundUser.Id, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundUser.Id, true);
                await _uow.CommitAsync();
            }

            //assign roles, claims & logins to users
            if (!await _uow.UserRepo.IsInLoginAsync(foundUser.Id, foundLogin.Id))
                await _uow.UserRepo.AddToLoginAsync(foundUser, foundLogin);

            if (!await _uow.UserRepo.IsInRoleAsync(foundUser.Id, foundRoleForAdmin.Id))
                await _uow.UserRepo.AddToRoleAsync(foundUser, foundRoleForAdmin);

            if (!await _uow.UserRepo.IsInRoleAsync(foundUser.Id, foundRoleForUser.Id))
                await _uow.UserRepo.AddToRoleAsync(foundUser, foundRoleForUser);
        }

        public async Task DestroyAsync()
        {
            //delete default users
            var userLocal = (await _uow.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin + "@local")).SingleOrDefault();

            if (userLocal != null)
            {
                await _uow.UserRepo.DeleteAsync(userLocal.Id);
                await _uow.CommitAsync();
            }

            //delete default roles
            var roleAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (roleAdmin != null)
            {
                await _uow.RoleRepo.DeleteAsync(roleAdmin.Id);
                await _uow.CommitAsync();
            }

            var roleUser = (await _uow.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForUser)).SingleOrDefault();

            if (roleUser != null)
            {
                await _uow.RoleRepo.DeleteAsync(roleUser.Id);
                await _uow.CommitAsync();
            }

            //delete default logins
            var loginLocal = (await _uow.LoginRepo.GetAsync(x => x.Name == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (loginLocal != null)
            {
                await _uow.LoginRepo.DeleteAsync(loginLocal.Id);
                await _uow.CommitAsync();
            }

            //delete default clients
            var clientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientApi)).SingleOrDefault();

            if (clientApi != null)
            {
                await _uow.ClientRepo.DeleteAsync(clientApi.Id);
                await _uow.CommitAsync();
            }

            var clientUi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).SingleOrDefault();

            if (clientUi != null)
            {
                await _uow.ClientRepo.DeleteAsync(clientUi.Id);
                await _uow.CommitAsync();
            }

            //delete default issuers
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).SingleOrDefault();

            if (issuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(issuer.Id);
                await _uow.CommitAsync();
            }
        }
    }
}
