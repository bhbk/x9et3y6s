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

namespace Bhbk.Lib.Identity.Data
{
    public class TestData
    {
        private readonly IIdentityContext<AppDbContext> _uow;

        public TestData(IIdentityContext<AppDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        public async void Create()
        {
            if (_uow.Situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            AppClient client;
            AppRole role;
            AppLogin login;
            AppUser user;

            //create clients
            await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(
                new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer1,
                    IssuerKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.IssuerRepo.CreateAsync(_uow.Convert.Map<AppIssuer>(
                new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer2,
                    IssuerKey = RandomValues.CreateBase64String(32),
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
                    ClientKey = RandomValues.CreateBase64String(32),
                    ClientType = Enums.ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.ClientRepo.CreateAsync(_uow.Convert.Map<AppClient>(
                new ClientCreate()
                {
                    IssuerId = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single().Id,
                    Name = Strings.ApiUnitTestClient2,
                    ClientKey = RandomValues.CreateBase64String(32),
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
            await _uow.CustomRoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(
                new RoleCreate()
                {
                    ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single().Id,
                    Name = Strings.ApiUnitTestRole1,
                    Enabled = true,
                    Immutable = false
                }));

            await _uow.CustomRoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(
                new RoleCreate()
                {
                    ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single().Id,
                    Name = Strings.ApiUnitTestRole2,
                    Enabled = true,
                    Immutable = false
                }));

            await _uow.CommitAsync();

            //create user A
            await _uow.CustomUserMgr.CreateAsync(_uow.Convert.Map<AppUser>(
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

            user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            await _uow.CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);
            await _uow.CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);
            await _uow.CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            //create user B
            await _uow.CustomUserMgr.CreateAsync(_uow.Convert.Map<AppUser>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser2,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    Immutable = false,
                }), Strings.ApiUnitTestUserPassCurrent);

            user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser2).Single();

            await _uow.CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);
            await _uow.CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);
            await _uow.CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);

            //assign roles, claims & logins to user A
            user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            await _uow.CustomUserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await _uow.CustomUserMgr.IsInRoleAsync(user, role.Name))
                await _uow.CustomUserMgr.AddToRoleAsync(user, role.Name);

            if (!await _uow.CustomUserMgr.IsInLoginAsync(user, login.LoginProvider))
                await _uow.CustomUserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign roles, claims & logins to user B
            user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser2).Single();
            role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin2)).Single();

            await _uow.CustomUserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await _uow.CustomUserMgr.IsInRoleAsync(user, role.Name))
                await _uow.CustomUserMgr.AddToRoleAsync(user, role.Name);

            if (!await _uow.CustomUserMgr.IsInLoginAsync(user, login.LoginProvider))
                await _uow.CustomUserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));
        }

        public async void CreateRandom(uint sets)
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
                        IssuerKey = RandomValues.CreateBase64String(32),
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
                        ClientKey = RandomValues.CreateBase64String(32),
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
                await _uow.CustomRoleMgr.CreateAsync(_uow.Convert.Map<AppRole>(
                    new RoleCreate()
                    {
                        ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == clientName)).Single().Id,
                        Name = roleName,
                        Enabled = true,
                        Immutable = false
                    }));

                await _uow.CommitAsync();

                //create random user
                await _uow.CustomUserMgr.CreateAsync(_uow.Convert.Map<AppUser>(
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
                user = _uow.CustomUserMgr.Store.Get(x => x.Email == userName).Single();
                role = _uow.CustomRoleMgr.Store.Get(x => x.Name == roleName).Single();
                login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == loginName)).Single();

                await _uow.CustomUserMgr.Store.SetEmailConfirmedAsync(user, true);
                await _uow.CustomUserMgr.Store.SetPasswordConfirmedAsync(user, true);
                await _uow.CustomUserMgr.Store.SetPhoneNumberConfirmedAsync(user, true);
                await _uow.CustomUserMgr.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

                if (!await _uow.CustomUserMgr.IsInRoleAsync(user, role.Name))
                    await _uow.CustomUserMgr.AddToRoleAsync(user, role.Name);

                if (!await _uow.CustomUserMgr.IsInLoginAsync(user, login.LoginProvider))
                    await _uow.CustomUserMgr.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));
            }
        }

        public async void Destroy()
        {
            if (_uow.Situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            foreach (var user in _uow.CustomUserMgr.Store.Get())
                await _uow.CustomUserMgr.DeleteAsync(user);

            foreach (var role in _uow.CustomRoleMgr.Store.Get())
                await _uow.CustomRoleMgr.DeleteAsync(role);

            await _uow.CommitAsync();

            foreach (var login in await _uow.LoginRepo.GetAsync())
                await _uow.LoginRepo.DeleteAsync(login);

            await _uow.CommitAsync();

            foreach (var client in await _uow.ClientRepo.GetAsync())
                await _uow.ClientRepo.DeleteAsync(client);

            await _uow.CommitAsync();

            foreach (var issuer in await _uow.IssuerRepo.GetAsync())
                await _uow.IssuerRepo.DeleteAsync(issuer);

            await _uow.CommitAsync();
        }
    }
}
