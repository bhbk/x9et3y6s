using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
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
    [Collection("StsTests")]
    public class RefreshTokenControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public RefreshTokenControllerTest(StartupTest factory)
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = true;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = true;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow;

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var pos = random.Next(((string)ok["refresh_token"]).Length - 8);

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), ((string)ok["refresh_token"]).Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var delete = await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            delete.Should().BeTrue();

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Fail_UserLocked()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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
            update.Should().BeAssignableTo(typeof(TUsers));

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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GetListV1((string)ok["access_token"], user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<RefreshModel>>();
            check.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], Guid.NewGuid().ToString());
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

            result = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], Guid.NewGuid().ToString());
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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

            result = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], user.Id.ToString(), Guid.NewGuid().ToString());
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<UserJwtV1>();
            check.Should().BeAssignableTo<UserJwtV1>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            claims.Value.Split(':')[1].Should().Be(salt);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            clients = new List<string> { client.Id.ToString() };

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            await _factory.UoW.ClientRepo.DeleteAsync(client.Id);
            await _factory.UoW.CommitAsync();

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_IssuerDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            await _factory.UoW.IssuerRepo.DeleteAsync(issuer.Id);
            await _factory.UoW.CommitAsync();

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var access_result = await _endpoints.RefreshToken_GenerateV2(string.Empty, clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var cc_result = await _endpoints.RefreshToken_GenerateV2(string.Empty, clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = true;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow;

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = true;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.UnitTestsPasswordRefresh = false;
            _factory.UoW.ConfigRepo.UnitTestsPasswordRefreshFakeUtcNow = DateTime.UtcNow;

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var access_pos = random.Next(((string)access_ok["refresh_token"]).Length - 8);

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, ((string)access_ok["refresh_token"]).Remove(access_pos, 8).Insert(access_pos, RandomValues.CreateBase64String(8)));
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());
            var cc_pos = random.Next(((string)cc_ok["refresh_token"]).Length - 8);

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, ((string)cc_ok["refresh_token"]).Remove(access_pos, 8).Insert(access_pos, RandomValues.CreateBase64String(8)));
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, string.Empty);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, string.Empty);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Fail_UserDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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

            await _factory.UoW.UserRepo.UpdateAsync(user);
            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var delete = await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            delete.Should().BeTrue();

            await _factory.UoW.CommitAsync();

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GetListV2((string)ok["access_token"], user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = JArray.Parse(await result.Content.ReadAsStringAsync()).ToObject<IEnumerable<RefreshModel>>();
            check.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], Guid.NewGuid().ToString());
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

            result = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], Guid.NewGuid().ToString());
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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

            result = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (await _factory.UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id
                && x.RefreshValue == (string)ok["refresh_token"])).Single();

            var result = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientEmpty()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var clients = new List<string> { string.Empty };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<UserJwtV2>();
            check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            claims.Value.Split(':')[1].Should().Be(salt);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientMultiple()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client1 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var client2 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client1.Id.ToString(), client2.Name };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            if (client1.Id == client2.Id)
                return;

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole1)).Single();
            await _factory.UoW.UserRepo.AddToRoleAsync(user, role);
            await _factory.UoW.CommitAsync();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)ok["refresh_token"]);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await result.Content.ReadAsStringAsync());

            var check = ok.ToObject<UserJwtV2>();
            check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            claims.Value.Split(':')[1].Should().Be(salt);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Success_ClientSingle()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var access = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());

            var access_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)access_ok["refresh_token"]);
            access_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access_result.StatusCode.Should().Be(HttpStatusCode.OK);

            access_ok = JObject.Parse(await access_result.Content.ReadAsStringAsync());

            var access_check = access_ok.ToObject<UserJwtV2>();
            access_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(access_check.access_token).Should().BeTrue();

            var access_claims = JwtBuilder.ReadJwtToken(access_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            access_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            access_claims.Value.Split(':')[1].Should().Be(salt);

            var cc = await _endpoints.ClientCredentials_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), client.ClientKey);
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var cc_result = await _endpoints.RefreshToken_GenerateV2(issuer.Id.ToString(), clients, (string)cc_ok["refresh_token"]);
            cc_result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_result.StatusCode.Should().Be(HttpStatusCode.OK);

            cc_ok = JObject.Parse(await cc_result.Content.ReadAsStringAsync());

            var cc_check = cc_ok.ToObject<ClientJwtV2>();
            cc_check.Should().BeAssignableTo<ClientJwtV2>();

            JwtBuilder.CanReadToken(cc_check.access_token).Should().BeTrue();

            var cc_claims = JwtBuilder.ReadJwtToken(cc_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            cc_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            cc_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
