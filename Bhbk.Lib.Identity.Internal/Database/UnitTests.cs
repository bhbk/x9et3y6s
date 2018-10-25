using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Database
{
    public class UnitTests
    {
        private readonly IIdentityContext _ioc;

        public UnitTests(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
        }

        public async void Create()
        {
            AppLogin login;
            AppUser user;
            AppRole role;
            AppAudience audience;

            //create clients
            await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = Strings.ApiUnitTestClient1,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = Strings.ApiUnitTestClient2,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            //create audiences
            await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single().Id,
                    Name = Strings.ApiUnitTestAudience1,
                    AudienceType = Enums.AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient2).Single().Id,
                    Name = Strings.ApiUnitTestAudience2,
                    AudienceType = Enums.AudienceType.server.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            //create roles
            await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single().Id,
                    Name = Strings.ApiUnitTestRole1,
                    Enabled = true,
                    Immutable = false
                }).Devolve());

            await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience2).Single().Id,
                    Name = Strings.ApiUnitTestRole2,
                    Enabled = true,
                    Immutable = false
                }).Devolve());

            //create logins
            await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin1,
                    Immutable = false
                }).Devolve());

            await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin2,
                    Immutable = false
                }).Devolve());

            //create user A
            await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser1,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }).Devolve(), Strings.ApiUnitTestUserPassCurrent);

            user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);

            //create user B
            await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = Strings.ApiUnitTestUser2,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    Immutable = false,
                }).Devolve(), Strings.ApiUnitTestUserPassCurrent);

            user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser2).Single();

            await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);

            //assign roles, claims & logins to user A
            user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            role = _ioc.RoleMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Strings.ApiUnitTestLogin1).Single();

            await _ioc.UserMgmt.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign roles, claims & logins to user B
            user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser2).Single();
            role = _ioc.RoleMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Strings.ApiUnitTestLogin2).Single();

            await _ioc.UserMgmt.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

            if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign uris to audience A
            audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();

            await _ioc.AudienceMgmt.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Strings.ApiUnitTestUri1,
                    AbsoluteUri = Strings.ApiUnitTestUri1Link,
                    Enabled = true,
                }).Devolve());

            //assign uris to audience B
            audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience2).Single();

            await _ioc.AudienceMgmt.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Strings.ApiUnitTestUri2,
                    AbsoluteUri = Strings.ApiUnitTestUri2Link,
                    Enabled = true,
                }).Devolve());
        }

        public async void CreateRandom(uint sets)
        {
            for (int i = 0; i < sets; i++)
            {
                AppLogin login;
                AppUser user;
                AppRole role;
                AppAudience audience;

                var clientName = Strings.ApiUnitTestClient1 + "-" + RandomValues.CreateBase64String(4);
                var audienceName = Strings.ApiUnitTestAudience1 + "-" + RandomValues.CreateBase64String(4);
                var roleName = Strings.ApiUnitTestRole1 + "-" + RandomValues.CreateBase64String(4);
                var loginName = Strings.ApiUnitTestLogin1 + "-" + RandomValues.CreateBase64String(4);
                var userName = RandomValues.CreateAlphaNumericString(4) + "-" + Strings.ApiUnitTestUser1;
                var uriName = Strings.ApiUnitTestUri1 + "-" + RandomValues.CreateBase64String(4);

                //create random client
                await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                    new ClientCreate()
                    {
                        Name = clientName,
                        ClientKey = RandomValues.CreateBase64String(32),
                        Enabled = true,
                        Immutable = false,
                    }).Devolve());

                //create random audience
                await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                    new AudienceCreate()
                    {
                        ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == clientName).Single().Id,
                        Name = audienceName,
                        AudienceType = Enums.AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }).Devolve());

                //create random role
                await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                    new RoleCreate()
                    {
                        AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == audienceName).Single().Id,
                        Name = roleName,
                        Enabled = true,
                        Immutable = false
                    }).Devolve());

                //create random login
                await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                    new LoginCreate()
                    {
                        LoginProvider = loginName,
                        Immutable = false
                    }).Devolve());

                //create random user
                await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                    new UserCreate()
                    {
                        Email = userName,
                        PhoneNumber = Strings.ApiDefaultPhone,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }).Devolve(), Strings.ApiUnitTestUserPassCurrent);

                //assign roles, claims & logins to random user
                user = _ioc.UserMgmt.Store.Get(x => x.Email == userName).Single();
                role = _ioc.RoleMgmt.Store.Get(x => x.Name == roleName).Single();
                login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == loginName).Single();

                await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(user, true);
                await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);
                await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);
                await _ioc.UserMgmt.AddClaimAsync(user, new Claim(Strings.ApiUnitTestClaimType, Strings.ApiUnitTestClaimValue));

                if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                    await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

                if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                    await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

                //assign uris to random audience
                audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == audienceName).Single();

                await _ioc.AudienceMgmt.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                    new AudienceUriCreate()
                    {
                        AudienceId = audience.Id,
                        Name = uriName,
                        AbsoluteUri = Strings.ApiUnitTestUri1Link,
                        Enabled = true,
                    }).Devolve());
            }
        }

        public async void DestroyAll()
        {
            foreach (var user in _ioc.UserMgmt.Store.Get())
                await _ioc.UserMgmt.DeleteAsync(user);

            foreach (var login in _ioc.LoginMgmt.Store.Get())
                await _ioc.LoginMgmt.DeleteAsync(login);

            foreach (var role in _ioc.RoleMgmt.Store.Get())
                await _ioc.RoleMgmt.DeleteAsync(role);

            foreach (var audience in _ioc.AudienceMgmt.Store.Get())
                await _ioc.AudienceMgmt.DeleteAsync(audience);

            foreach (var client in _ioc.ClientMgmt.Store.Get())
                await _ioc.ClientMgmt.DeleteAsync(client);
        }
    }
}
