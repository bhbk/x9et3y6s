using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTestCollection")]
    public class DeviceCodeControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public DeviceCodeControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Ask_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.DeviceCode_AskV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Generate_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.DeviceCode_UseV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), RandomValues.CreateBase64String(32));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Success()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_DeviceCodeV2_Generate_Fail()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_DeviceCodeV2_Generate_Success()
        {
            throw new NotImplementedException();
        }
    }
}
