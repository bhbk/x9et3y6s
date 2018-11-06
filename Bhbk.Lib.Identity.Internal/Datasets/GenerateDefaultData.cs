using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Datasets
{
    public class GenerateDefaultData
    {
        private readonly IIdentityContext<AppDbContext> _uow;

        public GenerateDefaultData(IIdentityContext<AppDbContext> uow)
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

                foundIssuer = await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(issuer));

                await _uow.CommitAsync();
            }

            var foundClientApi = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientApi)).SingleOrDefault();

            if (foundClientApi == null)
            {
                client = new ClientCreate()
                {
                    IssuerId = foundIssuer.Id,
                    Name = Strings.ApiDefaultClientApi,
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = Enums.ClientType.server.ToString(),
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
                    ClientType = Enums.ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = true,
                };

                foundClientUi = await _uow.ClientRepo.CreateAsync(client);

                await _uow.CommitAsync();
            }

            var foundLogin = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (foundLogin == null)
            {
                login = new LoginCreate()
                {
                    LoginProvider = Strings.ApiDefaultLogin,
                    Immutable = true,
                };

                foundLogin = await _uow.LoginRepo.CreateAsync(_uow.Convert.Map<AppLogin>(login));

                await _uow.CommitAsync();
            }

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

                await _uow.UserRepo.CreateAsync(_uow.Convert.Map<AppUser>(user), Strings.ApiDefaultUserPassword);
                await _uow.CommitAsync();

                foundUser = (await _uow.UserRepo.GetAsync(x => x.Email == user.Email)).Single();

                await _uow.UserRepo.SetConfirmedEmailAsync(foundUser, true);
                await _uow.UserRepo.SetConfirmedPasswordAsync(foundUser, true);
                await _uow.UserRepo.SetConfirmedPhoneNumberAsync(foundUser, true);
                await _uow.CommitAsync();
            }

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

                await _uow.RoleRepo.CreateAsync(_uow.Convert.Map<AppRole>(role));
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

                await _uow.RoleRepo.CreateAsync(_uow.Convert.Map<AppRole>(role));
                await _uow.CommitAsync();

                foundRoleForUser = (await _uow.RoleRepo.GetAsync(x => x.Name == role.Name)).Single();
            }

            if (!await _uow.UserRepo.IsInLoginAsync(foundUser, Strings.ApiDefaultLogin))
                await _uow.UserRepo.AddLoginAsync(foundUser,
                    new UserLoginInfo(Strings.ApiDefaultLogin, Strings.ApiDefaultLoginKey, Strings.ApiDefaultLoginName));

            if (!await _uow.UserRepo.IsInRoleAsync(foundUser, foundRoleForAdmin.Name))
                await _uow.UserRepo.AddToRoleAsync(foundUser, foundRoleForAdmin.Name);

            if (!await _uow.UserRepo.IsInRoleAsync(foundUser, foundRoleForUser.Name))
                await _uow.UserRepo.AddToRoleAsync(foundUser, foundRoleForUser.Name);
        }

        public async Task DestroyAsync()
        {
            var user = (await _uow.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin + "@local")).SingleOrDefault();

            if (user != null)
            {
                var roles = await _uow.UserRepo.GetRolesAsync(user);

                await _uow.UserRepo.RemoveFromRolesAsync(user, roles.ToArray());
                await _uow.CommitAsync();

                await _uow.UserRepo.DeleteAsync(user);
                await _uow.CommitAsync();
            }

            var roleAdmin = (await _uow.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForAdmin)).SingleOrDefault();

            if (roleAdmin != null)
            {
                await _uow.RoleRepo.DeleteAsync(roleAdmin);
                await _uow.CommitAsync();
            }

            var roleUser = (await _uow.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForUser)).SingleOrDefault();

            if (roleUser != null)
            {
                await _uow.RoleRepo.DeleteAsync(roleUser);
                await _uow.CommitAsync();
            }

            var loginLocal = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiDefaultLogin)).SingleOrDefault();

            if (loginLocal != null)
            {
                await _uow.LoginRepo.DeleteAsync(loginLocal);
                await _uow.CommitAsync();
            }

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

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).SingleOrDefault();

            if (issuer != null)
            {
                await _uow.IssuerRepo.DeleteAsync(issuer);
                await _uow.CommitAsync();
            }
        }
    }
}
