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
    [Collection("AlertTests")]
    public class TextControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AlertClient _endpoints;

        public TextControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AlertClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Api_TextV1_Enqueue_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.UserAccessTokenV2(_factory.UoW, issuer, new List<TClients> { client }, user);
            var response = await _endpoints.Enqueue_TextV1(access.token, new TextCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            response = await _endpoints.Enqueue_TextV1(access.token,
                new TextCreate()
                {
                    FromId = Guid.NewGuid(),
                    FromPhoneNumber = user.PhoneNumber,
                    ToId = user1.Id,
                    ToPhoneNumber = user1.PhoneNumber,
                    Body = Strings.ApiUnitTestTextBody,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response = await _endpoints.Enqueue_TextV1(access.token,
                new TextCreate()
                {
                    FromId = user.Id,
                    FromPhoneNumber = user1.PhoneNumber,
                    ToId = user1.Id,
                    ToPhoneNumber = user1.PhoneNumber,
                    Body = Strings.ApiUnitTestTextBody,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Api_TextV1_Enqueue_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.UserAccessTokenV2(_factory.UoW, issuer, new List<TClients> { client }, user);

            var response = await _endpoints.Enqueue_TextV1(access.token,
                new TextCreate()
                {
                    FromId = user.Id,
                    FromPhoneNumber = user.PhoneNumber,
                    ToId = user1.Id,
                    ToPhoneNumber = user1.PhoneNumber,
                    Body = Strings.ApiUnitTestTextBody,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
