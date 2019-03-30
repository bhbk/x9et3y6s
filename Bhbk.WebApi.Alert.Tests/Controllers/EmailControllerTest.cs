using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Alert;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.Controllers
{
    [Collection("AlertTestCollection")]
    public class EmailControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AlertClient _endpoints;

        public EmailControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AlertClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Api_EmailV1_Enqueue_Fail()
        {

        }

        [Fact]
        public async Task Api_EmailV1_Enqueue_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<TClients> { client }, user);

            var create = new EmailCreate()
            {
                FromId = Guid.NewGuid(),
                FromEmail = Strings.ApiUnitTestUser1,
                ToId = Guid.NewGuid(),
                ToEmail = Strings.ApiUnitTestUser1,
                Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
            };

            var response = await _endpoints.Enqueue_EmailV1(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
