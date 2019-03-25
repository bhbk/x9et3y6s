using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTestCollection")]
    public class RefreshTokenProviderTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public RefreshTokenProviderTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_ClientDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_ClientNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            await _factory.UoW.ClientRepo.DeleteAsync(client.Id);
            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_ClientUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), string.Empty, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_IssuerDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_IssuerNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            await _factory.UoW.IssuerRepo.DeleteAsync(issuer.Id);
            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_IssuerUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(string.Empty, client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_DateExpired()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = true;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_DateIssued()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = true;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_TokenNotValid()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var random = new Random();
            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var pos = random.Next(((string)ok["refresh_token"]).Length - 8);

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), ((string)ok["refresh_token"]).Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_TokenUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_UserNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var delete = await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            delete.Should().BeTrue();

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_UserLocked()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            var update = await _factory.UoW.UserRepo.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(AppUser));

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GetListV1(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<UserRefreshModel>>();
            check.Should().BeAssignableTo<IEnumerable<UserRefreshModel>>();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeAll_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteAllV1(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            result = await _endpoints.RefreshToken_DeleteAllV1(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeAll_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteAllV1(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeOne_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV1(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            result = await _endpoints.RefreshToken_DeleteV1(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString(), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeOne_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteAllV1(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Success_ClientSingle_ById()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV1>();
            check.Should().BeAssignableTo<JwtV1>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Success_ClientSingle_ByName()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Name, client.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Name, client.Name, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV1>();
            check.Should().BeAssignableTo<JwtV1>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_ClientDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            clients = new List<string> { client.Id.ToString() };

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_ClientNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            clients = new List<string> { RandomValues.CreateBase64String(8) };

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_IssuerNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(RandomValues.CreateBase64String(8), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_IssuerUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(string.Empty, clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_DateExpired()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = true;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_DateIssued()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = true;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsRefreshToken = false;
            _factory.UoW.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_TokenNotValid()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var random = new Random();
            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var pos = random.Next(((string)ok["refresh_token"]).Length - 8);

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, ((string)ok["refresh_token"]).Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_TokenUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_UserNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var delete = await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            delete.Should().BeTrue();

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_UserLocked()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await _factory.UoW.UserRepo.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(AppUser));

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GetListV2(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<UserRefreshModel>>();
            check.Should().BeAssignableTo<IEnumerable<UserRefreshModel>>();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeAll_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteAllV2(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            clients = new List<string> { client.Id.ToString() };
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            result = await _endpoints.RefreshToken_DeleteAllV2(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeAll_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteAllV2(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeOne_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV2(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            clients = new List<string> { client.Id.ToString() };
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            result = await _endpoints.RefreshToken_DeleteV2(new JwtSecurityToken((string)ok["access_token"]), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeOne_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (await _factory.UoW.UserRepo.GetRefreshAsync(x => x.UserId == user.Id)).First();

            var result = await _endpoints.RefreshToken_DeleteV2(new JwtSecurityToken((string)ok["access_token"]), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientMultiple_ById()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client1 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var client2 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client1.Id.ToString(), client2.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            if (client1.Id == client2.Id)
                return;

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole1)).Single();
            await _factory.UoW.UserRepo.AddToRoleAsync(user, role);
            await _factory.UoW.CommitAsync();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV2>();
            check.Should().BeAssignableTo<JwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientMultiple_ByName()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client1 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var client2 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client1.Name.ToString(), client2.Name.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            if (client1.Id == client2.Id)
                return;

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole1)).Single();
            await _factory.UoW.UserRepo.AddToRoleAsync(user, role);
            await _factory.UoW.CommitAsync();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Name, clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV2>();
            check.Should().BeAssignableTo<JwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientSingle_ById()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV2>();
            check.Should().BeAssignableTo<JwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientSingle_ByName()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Name };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Name, clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV2>();
            check.Should().BeAssignableTo<JwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var clients = new List<string> { string.Empty };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<JwtV2>();
            check.Should().BeAssignableTo<JwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();
        }
    }
}
