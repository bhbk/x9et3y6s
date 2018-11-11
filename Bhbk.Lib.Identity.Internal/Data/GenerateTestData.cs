using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Data
{
    public class GenerateTestData
    {
        private readonly IIdentityContext<AppDbContext> _uow;

        public GenerateTestData(IIdentityContext<AppDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async Task CreateAsync()
        {
            if (_uow.Situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            AppClient client;
            AppRole role;
            AppLogin login;
            AppUser user;

            //create issuers
            await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(
                new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer1,
                    IssuerKey = Strings.ApiUnitTestIssuer1Key,
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(
                new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer2,
                    IssuerKey = Strings.ApiUnitTestIssuer2Key,
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.CommitAsync();

            //create clients
            await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(
                new ClientCreate()
                {
                    IssuerId = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single().Id,
                    Name = Strings.ApiUnitTestClient1,
                    ClientKey = Strings.ApiUnitTestClient1Key,
                    ClientType = Enums.ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(
                new ClientCreate()
                {
                    IssuerId = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single().Id,
                    Name = Strings.ApiUnitTestClient2,
                    ClientKey = Strings.ApiUnitTestClient2Key,
                    ClientType = Enums.ClientType.server.ToString(),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.CommitAsync();

            //assign uris to clients
            client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            await _uow.ClientRepo.AddUriAsync(_uow.Convert.Map<AppClientUri>(
                new ClientUriCreate()
                {
                    ClientId = client.Id,
                    AbsoluteUri = Strings.ApiUnitTestUri1Link,
                    Enabled = true,
                }));

            client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();

            await _uow.ClientRepo.AddUriAsync(_uow.Convert.Map<AppClientUri>(
                new ClientUriCreate()
                {
                    ClientId = client.Id,
                    AbsoluteUri = Strings.ApiUnitTestUri2Link,
                    Enabled = true,
                }));

            await _uow.CommitAsync();

            //create logins
            await _uow.LoginRepo.CreateAsync(_uow.Convert.Map<AppLogin>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin1,
                    Immutable = false
                }));

            await _uow.LoginRepo.CreateAsync(_uow.Convert.Map<AppLogin>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin2,
                    Immutable = false
                }));

            await _uow.CommitAsync();

            //create roles
            await _uow.RoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(
                new RoleCreate()
                {
                    ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single().Id,
                    Name = Strings.ApiUnitTestRole1,
                    Enabled = true,
                    Immutable = false
                }));

            await _uow.RoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(
                new RoleCreate()
                {
                    ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single().Id,
                    Name = Strings.ApiUnitTestRole2,
                    Enabled = true,
                    Immutable = false
                }));

            await _uow.CommitAsync();

            //create user A
            await _uow.UserMgr.CreateAsync(_uow.Convert.Map<AppUser>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser1,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }), Strings.ApiUnitTestUserPassCurrent);

            user = (await _uow.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            await _uow.UserMgr.Store.SetEmailConfirmedAsync(user, true);
            await _uow.UserMgr.Store.SetPasswordConfirmedAsync(user, true);
            await _uow.UserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            //create user B
            await _uow.UserMgr.CreateAsync(_uow.Convert.Map<AppUser>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser2,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    Immutable = false,
                }), Strings.ApiUnitTestUserPassCurrent);

            user = (await _uow.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            await _uow.UserMgr.Store.SetEmailConfirmedAsync(user, true);
            await _uow.UserMgr.Store.SetPasswordConfirmedAsync(user, true);
            await _uow.UserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            //assign roles, claims & logins to user A
            user = (await _uow.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            role = (await _uow.RoleMgr.GetAsync(x => x.Name == Strings.ApiUnitTestRole1)).Single();
            login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            await _uow.UserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await _uow.UserMgr.IsInRoleAsync(user, role.Name))
                await _uow.UserMgr.AddToRoleAsync(user, role.Name);

            if (!await _uow.UserMgr.IsInLoginAsync(user, login.LoginProvider))
                await _uow.UserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign roles, claims & logins to user B
            user = (await _uow.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();
            role = (await _uow.RoleMgr.GetAsync(x => x.Name == Strings.ApiUnitTestRole2)).Single();
            login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin2)).Single();

            await _uow.UserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await _uow.UserMgr.IsInRoleAsync(user, role.Name))
                await _uow.UserMgr.AddToRoleAsync(user, role.Name);

            if (!await _uow.UserMgr.IsInLoginAsync(user, login.LoginProvider))
                await _uow.UserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            await _uow.CommitAsync();
        }

        public async Task CreateRandomAsync(uint sets)
        {
            if (_uow.Situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                AppClient client;
                AppRole role;
                AppLogin login;
                AppUser user;

                var issuerName = Strings.ApiUnitTestIssuer1 + "-" + RandomValues.CreateBase64String(4);
                var clientName = Strings.ApiUnitTestClient1 + "-" + RandomValues.CreateBase64String(4);
                var loginName = Strings.ApiUnitTestLogin1 + "-" + RandomValues.CreateBase64String(4);
                var roleName = Strings.ApiUnitTestRole1 + "-" + RandomValues.CreateBase64String(4);
                var userName = RandomValues.CreateAlphaNumericString(4) + "-" + Strings.ApiUnitTestUser1;
                var uriName = Strings.ApiUnitTestUri1 + "-" + RandomValues.CreateBase64String(4);

                //create random client
                await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(
                    new IssuerCreate()
                    {
                        Name = issuerName,
                        IssuerKey = Strings.ApiUnitTestIssuer1Key,
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                //create random client
                await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(
                    new ClientCreate()
                    {
                        IssuerId = (await _uow.IssuerRepo.GetAsync(x => x.Name == issuerName)).Single().Id,
                        Name = clientName,
                        ClientKey = Strings.ApiUnitTestClient1Key,
                        ClientType = Enums.ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                //assign uris to random client
                client = (await _uow.ClientRepo.GetAsync(x => x.Name == clientName)).Single();

                await _uow.ClientRepo.AddUriAsync(_uow.Convert.Map<AppClientUri>(
                    new ClientUriCreate()
                    {
                        ClientId = client.Id,
                        AbsoluteUri = Strings.ApiUnitTestUri1Link,
                        Enabled = true,
                    }));

                await _uow.CommitAsync();

                //create random login
                await _uow.LoginRepo.CreateAsync(_uow.Convert.Map<AppLogin>(
                    new LoginCreate()
                    {
                        LoginProvider = loginName,
                        Immutable = false
                    }));

                await _uow.CommitAsync();

                //create random role
                await _uow.RoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(
                    new RoleCreate()
                    {
                        ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == clientName)).Single().Id,
                        Name = roleName,
                        Enabled = true,
                        Immutable = false
                    }));

                await _uow.CommitAsync();

                //create random user
                await _uow.UserMgr.CreateAsync(_uow.Convert.Map<AppUser>(
                    new UserCreate()
                    {
                        Email = userName,
                        PhoneNumber = Strings.ApiDefaultPhone,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }), Strings.ApiUnitTestUserPassCurrent);

                //assign roles, claims & logins to random user
                user = (await _uow.UserMgr.GetAsync(x => x.Email == userName)).Single();
                role = (await _uow.RoleMgr.GetAsync(x => x.Name == roleName)).Single();
                login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == loginName)).Single();

                await _uow.UserMgr.Store.SetEmailConfirmedAsync(user, true);
                await _uow.UserMgr.Store.SetPasswordConfirmedAsync(user, true);
                await _uow.UserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);
                await _uow.UserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

                if (!await _uow.UserMgr.IsInRoleAsync(user, role.Name))
                    await _uow.UserMgr.AddToRoleAsync(user, role.Name);

                if (!await _uow.UserMgr.IsInLoginAsync(user, login.LoginProvider))
                    await _uow.UserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

                await _uow.CommitAsync();
            }
        }

        public async Task DestroyAsync()
        {
            if (_uow.Situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            var users = await _uow.UserMgr.GetAsync(x => x.Email.Contains(Strings.ApiUnitTestUser1)
                || x.Email.Contains(Strings.ApiUnitTestUser2));

            foreach(var user in users)
            {
                var userRoles = await _uow.UserMgr.GetRolesAsync(user);

                await _uow.UserMgr.RemoveFromRolesAsync(user, userRoles.ToArray());
                await _uow.UserMgr.DeleteAsync(user);
            }

            await _uow.CommitAsync();

            var roles = await _uow.RoleMgr.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestRole1)
                || x.Name.Contains(Strings.ApiUnitTestRole2));

            foreach(var role in roles)
                await _uow.RoleMgr.DeleteAsync(role);

            await _uow.CommitAsync();

            var logins = await _uow.LoginRepo.GetAsync(x => x.LoginProvider.Contains(Strings.ApiUnitTestLogin1)
                || x.LoginProvider.Contains(Strings.ApiUnitTestLogin2));

            foreach(var login in logins)
                await _uow.LoginRepo.DeleteAsync(login);

            await _uow.CommitAsync();

            var clients = await _uow.ClientRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestClient1)
                || x.Name.Contains(Strings.ApiUnitTestClient2));

            foreach(var client in clients)
                await _uow.ClientRepo.DeleteAsync(client);

            await _uow.CommitAsync();

            var issuers = await _uow.IssuerRepo.GetAsync(x => x.Name.Contains(Strings.ApiUnitTestIssuer1)
                || x.Name.Contains(Strings.ApiUnitTestIssuer2));

            foreach(var issuer in issuers)
                await _uow.IssuerRepo.DeleteAsync(issuer);

            await _uow.CommitAsync();
        }
    }
}
