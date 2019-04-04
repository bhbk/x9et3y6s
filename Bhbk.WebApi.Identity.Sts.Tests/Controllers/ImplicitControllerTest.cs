using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTests")]
    public class ImplicitControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public ImplicitControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_ImplicitV1_Ask_NotImplemented()
        {
            var ask = await _endpoints.Implicit_AskV1(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                RandomValues.CreateAlphaNumericString(8), RandomValues.CreateAlphaNumericString(8));
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_ImplicitV2_Ask_Fail()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_ImplicitV2_Ask_Success()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_ImplicitV1_Use_NotImplemented()
        {
            var ask = await _endpoints.Implicit_UseV1(RandomValues.CreateAlphaNumericString(8), RandomValues.CreateAlphaNumericString(8), RandomValues.CreateAlphaNumericString(8));
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_ImplicitV2_Use_Fail()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_ImplicitV2_Use_Success()
        {
            throw new NotImplementedException();
        }
    }
}
