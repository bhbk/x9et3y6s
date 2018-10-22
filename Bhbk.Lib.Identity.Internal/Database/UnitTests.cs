using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
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
                    Name = Statics.ApiUnitTestClientA,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            await _ioc.ClientMgmt.CreateAsync(new ClientFactory<ClientCreate>(
                new ClientCreate()
                {
                    Name = Statics.ApiUnitTestClientB,
                    ClientKey = RandomValues.CreateBase64String(32),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            //create audiences
            await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestClientA).Single().Id,
                    Name = Statics.ApiUnitTestAudienceA,
                    AudienceType = AudienceType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            await _ioc.AudienceMgmt.CreateAsync(new AudienceFactory<AudienceCreate>(
                new AudienceCreate()
                {
                    ClientId = _ioc.ClientMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestClientB).Single().Id,
                    Name = Statics.ApiUnitTestAudienceB,
                    AudienceType = AudienceType.server.ToString(),
                    Enabled = true,
                    Immutable = false,
                }).Devolve());

            //create roles
            await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestAudienceA).Single().Id,
                    Name = Statics.ApiUnitTestRoleA,
                    Enabled = true,
                    Immutable = false
                }).Devolve());

            await _ioc.RoleMgmt.CreateAsync(new RoleFactory<RoleCreate>(
                new RoleCreate()
                {
                    AudienceId = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestAudienceB).Single().Id,
                    Name = Statics.ApiUnitTestRoleB,
                    Enabled = true,
                    Immutable = false
                }).Devolve());

            //create logins
            await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = Statics.ApiUnitTestLoginA,
                    Immutable = false
                }).Devolve());

            await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = Statics.ApiUnitTestLoginB,
                    Immutable = false
                }).Devolve());

            //create user A
            await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = Statics.ApiUnitTestUserA,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                }).Devolve(), Statics.ApiUnitTestUserPassCurrent);

            user = _ioc.UserMgmt.Store.Get(x => x.Email == Statics.ApiUnitTestUserA).Single();

            await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);

            //create user B
            await _ioc.UserMgmt.CreateAsync(new UserFactory<UserCreate>(
                new UserCreate()
                {
                    Email = Statics.ApiUnitTestUserB,
                    PhoneNumber = Statics.ApiDefaultPhone,
                    FirstName = "First " + RandomValues.CreateBase64String(4),
                    LastName = "Last " + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    Immutable = false,
                }).Devolve(), Statics.ApiUnitTestUserPassCurrent);

            user = _ioc.UserMgmt.Store.Get(x => x.Email == Statics.ApiUnitTestUserB).Single();

            await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);
            await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);

            //assign roles, claims & logins to user A
            user = _ioc.UserMgmt.Store.Get(x => x.Email == Statics.ApiUnitTestUserA).Single();
            role = _ioc.RoleMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestRoleA).Single();
            login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiUnitTestLoginA).Single();

            await _ioc.UserMgmt.AddClaimAsync(user, new Claim(Statics.ApiUnitTestClaimType, Statics.ApiUnitTestClaimValue));

            if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign roles, claims & logins to user B
            user = _ioc.UserMgmt.Store.Get(x => x.Email == Statics.ApiUnitTestUserB).Single();
            role = _ioc.RoleMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestRoleB).Single();
            login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == Statics.ApiUnitTestLoginB).Single();

            await _ioc.UserMgmt.AddClaimAsync(user, new Claim(Statics.ApiUnitTestClaimType, Statics.ApiUnitTestClaimValue));

            if (!await _ioc.UserMgmt.IsInRoleAsync(user, role.Name))
                await _ioc.UserMgmt.AddToRoleAsync(user, role.Name);

            if (!await _ioc.UserMgmt.IsInLoginAsync(user, login.LoginProvider))
                await _ioc.UserMgmt.AddLoginAsync(user, new UserLoginInfo(login.LoginProvider, login.LoginProvider, "local"));

            //assign uris to audience A
            audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestAudienceA).Single();

            await _ioc.AudienceMgmt.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Statics.ApiUnitTestUriA,
                    AbsoluteUri = Statics.ApiUnitTestUriALink,
                    Enabled = true,
                }).Devolve());

            //assign uris to audience B
            audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Statics.ApiUnitTestAudienceB).Single();

            await _ioc.AudienceMgmt.AddUriAsync(new AudienceUriFactory<AudienceUriCreate>(
                new AudienceUriCreate()
                {
                    AudienceId = audience.Id,
                    Name = Statics.ApiUnitTestUriB,
                    AbsoluteUri = Statics.ApiUnitTestUriBLink,
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

                var clientName = Statics.ApiUnitTestClientA + "-" + RandomValues.CreateBase64String(4);
                var audienceName = Statics.ApiUnitTestAudienceA + "-" + RandomValues.CreateBase64String(4);
                var roleName = Statics.ApiUnitTestRoleA + "-" + RandomValues.CreateBase64String(4);
                var loginName = Statics.ApiUnitTestLoginA + "-" + RandomValues.CreateBase64String(4);
                var userName = RandomValues.CreateBase64String(4) + "-" + Statics.ApiUnitTestUserA;
                var uriName = Statics.ApiUnitTestUriA + "-" + RandomValues.CreateBase64String(4);

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
                        AudienceType = AudienceType.user_agent.ToString(),
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
                        PhoneNumber = Statics.ApiDefaultPhone,
                        FirstName = "First " + RandomValues.CreateBase64String(4),
                        LastName = "Last " + RandomValues.CreateBase64String(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }).Devolve(), Statics.ApiUnitTestUserPassCurrent);

                //assign roles, claims & logins to random user
                user = _ioc.UserMgmt.Store.Get(x => x.Email == userName).Single();
                role = _ioc.RoleMgmt.Store.Get(x => x.Name == roleName).Single();
                login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == loginName).Single();

                await _ioc.UserMgmt.Store.SetEmailConfirmedAsync(user, true);
                await _ioc.UserMgmt.Store.SetPasswordConfirmedAsync(user, true);
                await _ioc.UserMgmt.Store.SetPhoneNumberConfirmedAsync(user, true);
                await _ioc.UserMgmt.AddClaimAsync(user, new Claim(Statics.ApiUnitTestClaimType, Statics.ApiUnitTestClaimValue));

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
                        AbsoluteUri = Statics.ApiUnitTestUriALink,
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
