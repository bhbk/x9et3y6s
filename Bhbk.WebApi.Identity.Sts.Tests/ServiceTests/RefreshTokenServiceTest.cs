using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class RefreshTokenServiceTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public RefreshTokenServiceTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_GetListV1((string)ok["access_token"], user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = JArray.Parse(await rt.Content.ReadAsStringAsync()).ToObject<IEnumerable<RefreshModel>>();
            check.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Revoke_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            rt = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], user.Id.ToString(), Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Revoke_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            /*
             */
            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NoContent);

            /*
             */
            rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            rt = await _endpoints.RefreshToken_DeleteV1((string)ok["access_token"], user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = string.Empty,
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Use_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = string.Empty,
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Use_FailUser()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            /*
             */
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);
            await _factory.UoW.CommitAsync();

            var rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Use_FailToken()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var random = new Random();
            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            var pos = random.Next(((string)ok["refresh_token"]).Length - 8);

            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = ((string)ok["refresh_token"]).Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)),
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            /*
             */
            rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = string.Empty,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Use_Success_ClientSingle()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rt.Content.ReadAsStringAsync());

            var check = ok.ToObject<UserJwtV1>();
            check.Should().BeAssignableTo<UserJwtV1>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            claims.Value.Split(':')[1].Should().Be(salt);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_GetListV2((string)ok["access_token"], user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.OK);

            var check = JArray.Parse(await rt.Content.ReadAsStringAsync()).ToObject<IEnumerable<RefreshModel>>();
            check.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Revoke_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            rt = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], user.Id.ToString(), Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Revoke_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NoContent);

            rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            var refresh = (await _factory.UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id
                && x.RefreshValue == (string)ok["refresh_token"])).Single();

            rt = await _endpoints.RefreshToken_DeleteV2((string)ok["access_token"], user.Id.ToString(), refresh.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = string.Empty,
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = string.Empty,
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_FailToken()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var random = new Random();
            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            var rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            var rop_pos = random.Next(((string)rop_ok["refresh_token"]).Length - 8);

            cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());
            var cc_pos = random.Next(((string)cc_ok["refresh_token"]).Length - 8);

            rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = ((string)rop_ok["refresh_token"]).Remove(rop_pos, 8).Insert(rop_pos, RandomValues.CreateBase64String(8)),
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = ((string)cc_ok["refresh_token"]).Remove(rop_pos, 8).Insert(rop_pos, RandomValues.CreateBase64String(8)),
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            /*
             */
            rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            rop_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = string.Empty,
                });
            rop_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = string.Empty,
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_FailUser()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);
            await _factory.UoW.CommitAsync();

            var rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_Success_ClientEmpty()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rt.Content.ReadAsStringAsync());

            var check = ok.ToObject<UserJwtV2>();
            check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            claims.Value.Split(':')[1].Should().Be(salt);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_Success_ClientMultiple()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client1 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var client2 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            if (client1.Id == client2.Id)
                return;

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForUser)).Single();
            await _factory.UoW.UserRepo.AddToRoleAsync(user, role);
            await _factory.UoW.CommitAsync();

            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Id.ToString(), client2.Name }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Id.ToString(), client2.Name }),
                    grant_type = "refresh_token",
                    refresh_token = (string)ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await rt.Content.ReadAsStringAsync());

            var check = ok.ToObject<UserJwtV2>();
            check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(check.access_token).Should().BeTrue();

            var claims = JwtBuilder.ReadJwtToken(check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            claims.Value.Split(':')[1].Should().Be(salt);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_Use_Success_ClientSingle()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            /*
             */
            var rop = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());

            var rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop_ok["refresh_token"],
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.OK);

            var rt_ok = JObject.Parse(await rt.Content.ReadAsStringAsync());

            var rt_check = rt_ok.ToObject<UserJwtV2>();
            rt_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(rt_check.access_token).Should().BeTrue();

            var rt_claims = JwtBuilder.ReadJwtToken(rt_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            rt_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            rt_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            var cc = await _endpoints.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.OK);

            var cc_ok = JObject.Parse(await cc.Content.ReadAsStringAsync());

            var cc_rt = await _endpoints.RefreshToken_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)cc_ok["refresh_token"],
                });
            cc_rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc_rt.StatusCode.Should().Be(HttpStatusCode.OK);

            cc_ok = JObject.Parse(await cc_rt.Content.ReadAsStringAsync());

            var cc_check = cc_ok.ToObject<ClientJwtV2>();
            cc_check.Should().BeAssignableTo<ClientJwtV2>();

            JwtBuilder.CanReadToken(cc_check.access_token).Should().BeTrue();

            var cc_claims = JwtBuilder.ReadJwtToken(cc_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            cc_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            cc_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
