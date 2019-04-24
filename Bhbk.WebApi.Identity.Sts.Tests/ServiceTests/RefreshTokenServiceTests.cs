using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class RefreshTokenServiceTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly IStsService _service;

        public RefreshTokenServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _service = new StsService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_GetList_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });

            var rt = await _service.Repo.RefreshToken_GetListV1(rop.access_token, user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_Revoke_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });

            var rt = await _service.Repo.RefreshToken_DeleteAllV1(rop.access_token, Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotImplemented);

            rt = await _service.Repo.RefreshToken_DeleteV1(rop.access_token, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });

            var rt = await _service.Repo.RefreshToken_GetListV2(rop.access_token, user.Id.ToString());
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

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            var rt = await _service.Repo.RefreshToken_DeleteAllV2(rop.access_token, Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

            rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });

            rt = await _service.Repo.RefreshToken_DeleteAllV2(rop.access_token, Guid.NewGuid().ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _service.Repo.RefreshToken_DeleteV2(rop.access_token, user.Id.ToString(), Guid.NewGuid().ToString());
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

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });

            var rt = await _service.Repo.RefreshToken_DeleteAllV2(rop.access_token, user.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NoContent);

            rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiDefaultAdminUserPassword,
                });

            var refresh = (await _factory.UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id
                && x.RefreshValue == rop.refresh_token)).Single();

            rt = await _service.Repo.RefreshToken_DeleteV2(rop.access_token, user.Id.ToString(), refresh.Id.ToString());
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
