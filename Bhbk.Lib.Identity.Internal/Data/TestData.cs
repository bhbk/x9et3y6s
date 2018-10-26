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

        public async void TestsCreate()
        {
            if (_uow.Situation != ContextType.UnitTest)
                throw new InvalidOperationException();

            AppLogin login;
            AppUser user;
            AppRole role;
            AppAudience audience;

            //create clients
            await _uow.ClientRepo.CreateAsync(_uow.Maps.Map<AppClient>(
                new ClientCreate()
                {
                    Name = Strings.ApiUnitTestClient1,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.ClientRepo.CreateAsync(_uow.Maps.Map<AppClient>(
                new ClientCreate()
                {
                    Name = Strings.ApiUnitTestClient2,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.CommitAsync();

            //create audiences
            await _uow.AudienceRepo.CreateAsync(_uow.Maps.Map<AppAudience>(
                new AudienceCreate()
                {
                    ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single().Id,
                    Name = Strings.ApiUnitTestAudience1,
                    AudienceType = Enums.AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.AudienceRepo.CreateAsync(_uow.Maps.Map<AppAudience>(
                new AudienceCreate()
                {
                    ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single().Id,
                    Name = Strings.ApiUnitTestAudience2,
                    AudienceType = Enums.AudienceType.server.ToString(),
                    Enabled = true,
                    Immutable = false,
                }));

            await _uow.CommitAsync();

            //assign uris to audiences
            audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();

            await _uow.AudienceRepo.AddUriAsync(_uow.Maps.Map<AppAudienceUri>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Strings.ApiUnitTestUri1,
                    AbsoluteUri = Strings.ApiUnitTestUri1Link,
                    Enabled = true,
                }));

            audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience2)).Single();

            await _uow.AudienceRepo.AddUriAsync(_uow.Maps.Map<AppAudienceUri>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Strings.ApiUnitTestUri2,
                    AbsoluteUri = Strings.ApiUnitTestUri2Link,
                    Enabled = true,
                }));

            await _uow.CommitAsync();

            //create logins
            await _uow.LoginRepo.CreateAsync(_uow.Maps.Map<AppLogin>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin1,
                    Immutable = false
                }));

            await _uow.LoginRepo.CreateAsync(_uow.Maps.Map<AppLogin>(
                new LoginCreate()
                {
                    LoginProvider = Strings.ApiUnitTestLogin2,
                    Immutable = false
                }));

            await _uow.CommitAsync();

            //create roles
            await _uow.CustomRoleMgr.CreateAsync(_uow.Maps.Map<AppRole>(
                new RoleCreate()
                {
                    AudienceId = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single().Id,
                    Name = Strings.ApiUnitTestRole1,
                    Enabled = true,
                    Immutable = false
                }));

            await _uow.CustomRoleMgr.CreateAsync(_uow.Maps.Map<AppRole>(
                new RoleCreate()
                {
                    AudienceId = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience2)).Single().Id,
                    Name = Strings.ApiUnitTestRole2,
                    Enabled = true,
                    Immutable = false
                }));

            await _uow.CommitAsync();

            //create user A
            await _uow.CustomUserMgr.CreateAsync(_uow.Maps.Map<AppUser>(
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
            await _uow.CustomUserMgr.CreateAsync(_uow.Maps.Map<AppUser>(
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
                AppLogin login;
                AppUser user;
                AppRole role;
                AppAudience audience;

                var clientName = Strings.ApiUnitTestClient1 + "-" + RandomValues.CreateBase64String(4);
                var audienceName = Strings.ApiUnitTestAudience1 + "-" + RandomValues.CreateBase64String(4);
                var loginName = Strings.ApiUnitTestLogin1 + "-" + RandomValues.CreateBase64String(4);
                var roleName = Strings.ApiUnitTestRole1 + "-" + RandomValues.CreateBase64String(4);
                var userName = RandomValues.CreateAlphaNumericString(4) + "-" + Strings.ApiUnitTestUser1;
                var uriName = Strings.ApiUnitTestUri1 + "-" + RandomValues.CreateBase64String(4);

                //create random client
                await _uow.ClientRepo.CreateAsync(_uow.Maps.Map<AppClient>(
                    new ClientCreate()
                    {
                        Name = clientName,
                        ClientKey = RandomValues.CreateBase64String(32),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                //create random audience
                await _uow.AudienceRepo.CreateAsync(_uow.Maps.Map<AppAudience>(
                    new AudienceCreate()
                    {
                        ClientId = (await _uow.ClientRepo.GetAsync(x => x.Name == clientName)).Single().Id,
                        Name = audienceName,
                        AudienceType = Enums.AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                await _uow.CommitAsync();

                //assign uris to random audience
                audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == audienceName)).Single();

                await _uow.AudienceRepo.AddUriAsync(_uow.Maps.Map<AppAudienceUri>(
                    new AudienceUriCreate()
                    {
                        AudienceId = audience.Id,
                        Name = uriName,
                        AbsoluteUri = Strings.ApiUnitTestUri1Link,
                        Enabled = true,
                    }));

                await _uow.CommitAsync();

                //create random login
                await _uow.LoginRepo.CreateAsync(_uow.Maps.Map<AppLogin>(
                    new LoginCreate()
                    {
                        LoginProvider = loginName,
                        Immutable = false
                    }));

                await _uow.CommitAsync();

                //create random role
                await _uow.CustomRoleMgr.CreateAsync(_uow.Maps.Map<AppRole>(
                    new RoleCreate()
                    {
                        AudienceId = (await _uow.AudienceRepo.GetAsync(x => x.Name == audienceName)).Single().Id,
                        Name = roleName,
                        Enabled = true,
                        Immutable = false
                    }));

                await _uow.CommitAsync();

                //create random user
                await _uow.CustomUserMgr.CreateAsync(_uow.Maps.Map<AppUser>(
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

            foreach (var audience in await _uow.AudienceRepo.GetAsync())
                await _uow.AudienceRepo.DeleteAsync(audience);

            await _uow.CommitAsync();

            foreach (var client in await _uow.ClientRepo.GetAsync())
                await _uow.ClientRepo.DeleteAsync(client);

            await _uow.CommitAsync();
        }
    }
}
